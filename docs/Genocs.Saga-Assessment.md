# Genocs.Saga Assessment

## Executive Summary

## Status Update

Implemented in the current workspace state:

- `SAGA-001` durable step outcome tracking for saga log entries
- `SAGA-002` compensation replay limited to previously completed steps
- `SAGA-003` per-saga execution context isolation during parallel fan-out
- `SAGA-004` completed sagas are now terminal by default
- `SAGA-005` in-memory saga state and log stores are thread-safe for concurrent use
- `SAGA-006` safe default persistence seeding for `AddSaga(...)`
- `SAGA-007` explicit versioning on saga state contracts
- `SAGA-008` optimistic concurrency semantics in state repositories
- `SAGA-009` baseline duplicate-delivery handling via explicit message identity
- `SAGA-010` explicit compensation lifecycle states and durable failure markers
- `SAGA-011` configurable in-process execution locking with durable stores as the correctness boundary
- `SAGA-016` explicit assembly registration for deterministic saga discovery

Still open from the highest-priority set at the time of writing:

None in the original phase-one safety set, and the first persistence hardening slice is now in place.

`Genocs.Saga` is a useful starting point for an in-process saga orchestration library, but it is not yet production-hardened.

The core strengths are:

- a small public surface
- simple DI registration through `AddSaga(...)`
- a clear split between coordinator, initializer, processor, and post-processor
- pluggable persistence contracts for state and log storage
- trace-context support for correlation with distributed telemetry

The main remaining risks are now concentrated in distributed-runtime behavior and operational maturity:

- callback ordering still exposes `Rejected` before compensation has completed
- duplicate delivery handling is now defined as opt-in message-identity deduplication, but it is not yet a distributed exactly-once guarantee
- compensation failure is now durable and queryable, but there is still no built-in retry or operator recovery workflow
- convenience discovery still depends on ambient `AppDomain` loading, but hosts can now bypass that by supplying explicit assemblies

My assessment is that the library has moved from starter-kit territory into a credible orchestration core for controlled environments, but it is still not a fully reliable distributed workflow foundation for high-value production paths.

---

## Current Design Snapshot

At a high level the runtime flow is:

1. `ISagaCoordinator.ProcessAsync(...)` resolves all saga handlers for a message.
2. Each matching saga runs concurrently.
3. Each saga resolves an id through `ISaga.ResolveId(...)`.
4. The initializer loads state or creates it for start actions.
5. The processor executes `HandleAsync(...)`.
6. State and log entries are persisted.
7. The post-processor invokes completion or rejection hooks.
8. Rejected sagas replay log entries in reverse order through `CompensateAsync(...)`.

This is a reasonable structure, but the failure and concurrency details need tightening.

---

## Current Risk Surface After `SAGA-010`

The highest-severity correctness defects from the first assessment pass are addressed in the current workspace state.

That changes the architecture posture in two important ways:

- the base runtime now has explicit state versioning and can detect stale writes instead of silently overwriting state
- the in-memory implementation now behaves much closer to a real repository by returning snapshots rather than shared mutable references

The remaining concerns are mostly about distributed guarantees and lifecycle completeness rather than basic local correctness:

1. Duplicate delivery handling is now explicitly opt-in through `ISagaMessageIdentity` or `SagaContextMetadataKeys.MessageId`, but cross-node exactly-once semantics still depend on the persistence backend.
2. Compensation failure is now persisted as `CompensationFailed`, and the coordinator now offers a baseline retry path, but broader operator workflows are still undefined.
3. Discovery diagnostics and lifecycle policy remain under-specified for larger modular deployments.
4. In-process locking is now explicitly configurable and treated as a local optimization rather than a distributed guarantee.

---

## Findings

Findings 1 through 5 are retained for traceability, but they are resolved in the current workspace state.

### 1. Compensation includes the failing step, not just previously completed steps

**Status**: Resolved by `SAGA-001` and `SAGA-002`

**Severity**: Critical

**Why it matters**

In the current implementation, the processor writes a log entry in the `finally` block regardless of whether `HandleAsync(...)` succeeded or threw. The post-processor later compensates every logged message in reverse order. That means the step that actually failed is still recorded as compensatable work.

This breaks a core saga guarantee. Compensation should usually revert completed side effects, not attempt to undo a step that may have failed before committing anything.

**Evidence**

- [src/Genocs.Saga/Managers/SagaProcessor.cs](src/Genocs.Saga/Managers/SagaProcessor.cs#L50)
- [src/Genocs.Saga/Managers/SagaProcessor.cs](src/Genocs.Saga/Managers/SagaProcessor.cs#L68)
- [src/Genocs.Saga/Managers/SagaPostProcessor.cs](src/Genocs.Saga/Managers/SagaPostProcessor.cs#L41)

**Impact**

- compensation can execute for a step that never committed successfully
- compensators must defensively handle operations that may not have happened
- rollback semantics become ambiguous and handler authors have to guess library behavior

**Recommended fix**

- Persist step execution outcome explicitly, not just the message.
- Log only successfully committed compensatable steps, or store a `StepStatus` such as `Started`, `Completed`, `Failed`, `Compensated`.
- Change compensation replay to include only successfully completed steps.

---

### 2. Parallel saga fan-out shares a single mutable context instance

**Status**: Resolved by `SAGA-003`

**Severity**: High

**Why it matters**

When multiple saga classes handle the same message type, the coordinator starts them in parallel with `Task.WhenAll(...)` and passes the same `ISagaContext` instance to every saga task. That context contains mutable error state through `SagaContextError`.

This creates cross-saga contamination. One saga can overwrite error information used by another, and telemetry or hooks can observe a context mutated by a different handler.

**Evidence**

- [src/Genocs.Saga/Managers/SagaCoordinator.cs](src/Genocs.Saga/Managers/SagaCoordinator.cs#L46)
- [src/Genocs.Saga/Managers/SagaCoordinator.cs](src/Genocs.Saga/Managers/SagaCoordinator.cs#L51)
- [src/Genocs.Saga/Managers/SagaProcessor.cs](src/Genocs.Saga/Managers/SagaProcessor.cs#L35)
- [src/Genocs.Saga/SagaContext.cs](src/Genocs.Saga/SagaContext.cs#L10)

**Impact**

- race conditions in error reporting
- incorrect telemetry outcome tags
- hard-to-debug behavior when multiple sagas react to the same message

**Recommended fix**

- Treat `ISagaContext` as immutable input.
- Move mutable execution state into a per-saga execution object such as `SagaExecutionContext`.
- If fan-out remains parallel, clone or fork the incoming context per saga instance.

---

### 3. Completed sagas are still processable, so completion is not terminal

**Status**: Resolved by `SAGA-004`

**Severity**: High

**Why it matters**

The initializer blocks only rejected sagas. A saga already marked `Completed` is loaded and can continue processing future messages. That may be intentional for some workflows, but it is not a safe default for a general-purpose saga library.

Most teams expect completed workflows to be terminal unless they explicitly opt into reopening or multi-stage completion semantics.

**Evidence**

- [src/Genocs.Saga/Managers/SagaInitializer.cs](src/Genocs.Saga/Managers/SagaInitializer.cs#L32)

**Impact**

- duplicate external side effects after a workflow was considered finished
- ambiguous lifecycle semantics for consumers
- no reliable “finalized” state boundary for persistence, monitoring, or cleanup

**Recommended fix**

- Make terminal-state behavior explicit.
- By default, block both `Completed` and `Rejected` sagas from reprocessing.
- If reopening is required, expose a policy such as `ISagaLifecyclePolicy` or a configurable terminal-state option.

---

### 4. The in-memory repositories are not thread-safe across different saga ids

**Status**: Resolved by `SAGA-005`

**Severity**: High

**Why it matters**

The keyed locker serializes processing by saga id, but the in-memory state store and log store are backed by plain `List<T>` collections with no synchronization. Two different saga ids can execute concurrently and mutate those lists at the same time.

That makes the default implementation unsafe even for local integration testing under parallel load.

**Evidence**

- [src/Genocs.Saga/Async/KeyedLocker.cs](src/Genocs.Saga/Async/KeyedLocker.cs#L29)
- [src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs](src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs#L5)
- [src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs](src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs#L16)
- [src/Genocs.Saga/Persistence/InMemorySagaLog.cs](src/Genocs.Saga/Persistence/InMemorySagaLog.cs#L5)
- [src/Genocs.Saga/Persistence/InMemorySagaLog.cs](src/Genocs.Saga/Persistence/InMemorySagaLog.cs#L15)

**Impact**

- nondeterministic test failures
- corrupted in-memory state/log behavior under parallel execution
- false confidence from development setups that behave differently from durable stores

**Recommended fix**

- Replace `List<T>` with a thread-safe structure, or protect the collections with locks.
- Prefer `ConcurrentDictionary<(SagaId, Type), ISagaState>` for state.
- Prefer a thread-safe append-only structure or a locked dictionary of per-saga log lists for logs.

---

### 5. `AddSaga(Action<ISagaBuilder>)` is easy to misconfigure into an invalid runtime

**Status**: Resolved by `SAGA-006`

**Severity**: Medium

**Why it matters**

The parameterless overload adds in-memory persistence. The callback overload does not. If the callback is empty, or if it registers only state or only log persistence, the runtime is left partially configured and fails only when messages are processed.

This is a usability pitfall that will produce avoidable runtime failures.

**Evidence**

- [src/Genocs.Saga/Extensions.cs](src/Genocs.Saga/Extensions.cs#L20)
- [src/Genocs.Saga/Extensions.cs](src/Genocs.Saga/Extensions.cs#L22)

**Impact**

- latent runtime failures instead of startup validation
- subtle breakage when developers add a callback only to customize one option

**Recommended fix**

- Always seed in-memory persistence first, then let the callback override it.
- Or validate at startup that both `ISagaStateRepository` and `ISagaLog` are registered exactly once.
- Add tests for empty callback and partial callback registration.

---

### 6. Saga discovery depends on assemblies already loaded into the AppDomain

**Severity**: Medium

**Why it matters**

Registration scans `AppDomain.CurrentDomain.GetAssemblies()`. That means discovery depends on which assemblies happen to be loaded at startup time. Saga implementations in referenced packages may be missed if nothing has forced them to load yet.

This becomes a real pitfall as soon as the library is used across multiple projects or plugin-style modules.

**Evidence**

- [src/Genocs.Saga/Extensions.cs](src/Genocs.Saga/Extensions.cs#L37)

**Impact**

- nondeterministic registration behavior
- hard-to-diagnose “saga not found” problems in modular solutions

**Recommended fix**

- Allow explicit assembly registration: `AddSaga(params Assembly[] assemblies)`.
- Keep the current scan as a convenience overload, not the only discovery strategy.
- Add startup diagnostics that list discovered saga types.

---

### 7. Safe-by-default correlation is weak when callers pass `null` context

**Severity**: Medium

**Why it matters**

If no context is supplied, the coordinator uses `SagaContext.Empty`, which creates a brand-new saga id and an empty originator. The default `ResolveId(...)` implementation also reads the id from the context.

That means the default behavior is effectively “new workflow every time” unless the consumer already knows to supply a properly built context or override `ResolveId(...)`.

**Evidence**

- [src/Genocs.Saga/SagaContext.cs](src/Genocs.Saga/SagaContext.cs#L27)
- [src/Genocs.Saga/Saga.cs](src/Genocs.Saga/Saga.cs#L12)
- [src/Genocs.Saga/Managers/SagaCoordinator.cs](src/Genocs.Saga/Managers/SagaCoordinator.cs#L63)

**Impact**

- accidental saga fragmentation
- empty originator metadata in telemetry and audits
- surprising behavior for new consumers

**Recommended fix**

- Make correlation strategy explicit in the API.
- Consider rejecting `null` context unless a saga provides its own correlation resolver.
- Add a first-class abstraction such as `ISagaIdResolver<TMessage>`.

---

### 8. Rejection callback executes before compensation completes

**Severity**: Medium

**Why it matters**

The rejection hook runs before the library performs compensation. External observers can therefore react to “rejected” state while the rollback is still in progress.

This may be acceptable in some systems, but it should be explicit and preferably configurable.

**Evidence**

- [src/Genocs.Saga/Managers/SagaPostProcessor.cs](src/Genocs.Saga/Managers/SagaPostProcessor.cs#L26)
- [src/Genocs.Saga/Managers/SagaPostProcessor.cs](src/Genocs.Saga/Managers/SagaPostProcessor.cs#L27)

**Impact**

- external notifications may be emitted before consistency is restored
- confusing semantics for failure handlers

**Recommended fix**

- Split failure hooks into `OnRejected` and `OnCompensated`, or run rejection notifications after compensation by default.
- Persist compensation status so observers can distinguish “rejected” from “rolled back”.

---

### 9. Duplicate delivery handling is now defined, but still best-effort across nodes

**Status**: Partially resolved by `SAGA-009`

**Severity**: Medium

**Why it matters**

The runtime now supports explicit message identity and skips repeated deliveries for the same saga instance when a message implements `ISagaMessageIdentity` or the caller supplies `SagaContextMetadataKeys.MessageId`.

That closes the “undefined behavior” gap for normal opt-in usage, but it is still not a complete exactly-once guarantee across competing nodes because the current log contracts do not reserve a message id before side effects begin.

**Impact**

- duplicate-delivery behavior is now predictable for normal opt-in usage
- multi-node races can still produce side effects before a duplicate is observed in shared persistence

**Recommended next step**

- add a durable reservation or compare-and-set model for message identity in persistence adapters that need stronger guarantees

---

### 10. Compensation failure is durable, but recovery policy is still limited

**Status**: Partially resolved by `SAGA-010`

**Severity**: Medium

**Why it matters**

The runtime now persists `Compensating`, `Compensated`, and `CompensationFailed` states and updates saga log outcomes as compensation succeeds or fails. That removes the previous blind spot where rollback errors disappeared into transient process memory.

The remaining gap is operational policy breadth rather than visibility.

**Impact**

- failed rollback is now observable and queryable
- there is now a baseline retry path for failed compensation, but no richer resume, archive, or workflow-specific intervention model

**Recommended next step**

- expand recovery operations beyond the new retry-compensation baseline and define policy for operator intervention

---

### 11. The keyed locker is no longer a hidden correctness dependency

**Status**: Resolved by `SAGA-011`

**Severity**: Medium

**Why it matters**

The runtime previously serialized all work for a saga id through a process-local static keyed locker. That improved local determinism, but it also blurred where correctness actually came from.

The runtime now uses an explicit execution-lock abstraction with two important properties:

- the default remains in-process locking for local efficiency and reduced contention
- hosts can disable it and still rely on repository concurrency semantics for correctness

**Impact**

- local serialization is now an implementation choice rather than an accidental guarantee
- distributed correctness is documented around persistence concurrency instead of process memory

**Recommended next step**

- add diagnostics that expose which execution-lock mode is active at startup

---

### 12. Saga discovery is now deterministic when hosts opt in

**Status**: Resolved by `SAGA-016`

**Severity**: Medium

**Why it matters**

Registration no longer depends exclusively on whatever assemblies happen to already be loaded into the `AppDomain`. Hosts can now call `AddSaga(params Assembly[] assemblies)` or `AddSaga(Action<ISagaBuilder>, params Assembly[] assemblies)` to make discovery deterministic.

**Impact**

- modular hosts can register saga assemblies deliberately
- discovery no longer has to rely on incidental load order when the host wants explicit control

**Recommended next step**

- add startup diagnostics that report which assemblies were scanned and which saga/message bindings were discovered

---

## Additional Gaps and Missing Capabilities

These are not all bugs, but they are the main reasons the library remains an early-stage implementation rather than a mature saga platform.

### Reliability gaps

- no retry policy or retry classification for transient failures
- no timeout or scheduled-message support for long-running sagas
- no durable reservation model for duplicate message delivery across competing nodes

### Operational gaps

- no startup validation report for discovered sagas and registered persistence providers
- no startup report of which assemblies were scanned for saga discovery
- no built-in metrics for started, completed, rejected, compensated, or compensation-failed workflows
- no administration surface for querying current saga state
- no cleanup or archival policy for finished sagas
- no startup diagnostics for whether the host is using local execution locking or durable-store-only coordination

### Evolution gaps

- no explicit serializer/versioning strategy for persisted state and log payloads
- no migration path for changing message contracts over time
- no workflow-definition model or validation to prevent invalid state transitions

### Developer-experience gaps

- no first-class test harness for deterministic saga simulation
- no support for step policies such as retry, timeout, non-compensatable, or forward-only step
- no capability map for transport integration patterns such as outbox/inbox coupling

---

## Recommended Roadmap

### Phase 1: Correctness and Safety

**Status**: Complete in the current workspace state

**Goal**: Remove semantics that can produce incorrect workflow outcomes.

1. Redesign log persistence so it records step outcome, not just message payload.
2. Compensate only successfully completed steps.
3. Make execution state per-saga and keep the input context immutable.
4. Make completed state terminal by default.
5. Make in-memory repositories thread-safe.
6. Add startup validation for persistence registration.

**Definition of done**

- compensation excludes failed current steps
- parallel fan-out does not share mutable state
- terminal-state behavior is explicit and tested
- local concurrency tests pass reliably

### Phase 2: Persistence and Concurrency Hardening

**Goal**: Make the runtime safe under multiple instances and duplicate delivery.

1. Add versioning to `ISagaState` and repository contracts. Completed.
2. Introduce optimistic concurrency checks in durable persistence providers. Completed for the built-in, MongoDB, and Redis adapters.
3. Add idempotency keys or message deduplication support. Completed as a baseline opt-in implementation.
4. Revisit the in-process `KeyedLocker` so distributed correctness depends on persistence concurrency guarantees, not local locks. Completed.
5. Add compensation failure handling and durable failure markers. Completed.

**Definition of done**

- duplicate message handling is explicit and baseline-tested
- concurrent updates fail predictably instead of silently overwriting
- compensation failure has an observable state and a baseline retry path, but broader recovery tooling is still pending
- in-process locking is configurable and documented as an optimization rather than a correctness boundary

### Phase 3: Lifecycle and Policy Model

**Goal**: Move from “handler-based starter” to “workflow engine with explicit policies”.

1. Add a lifecycle policy abstraction for terminal states and reopening.
2. Add per-step policies for retry, timeout, compensatable vs forward-only behavior, and maximum attempts.
3. Persist richer workflow states such as `Compensating`, `Compensated`, and `CompensationFailed`.
4. Add scheduled timeout messages for long-running sagas.

**Definition of done**

- workflow behavior is configurable without changing each saga implementation
- failure states are operationally visible

### Phase 4: Discovery, Diagnostics, and Tooling

**Goal**: Improve adoption and reduce integration surprises.

1. Support explicit assembly registration for saga discovery.
2. Emit a startup report of discovered sagas and bound message types.
3. Add metrics for saga counts and durations.
4. Add a diagnostic API or query service for current state inspection.
5. Add a deterministic testing harness for end-to-end saga scenarios.

**Definition of done**

- missing discovery problems are diagnosable at startup
- operators can inspect runtime state and outcomes
- developers can test sagas without mocking the coordinator by hand

### Phase 5: Advanced Capabilities

**Goal**: Position the library as a serious distributed workflow component.

1. Add inbox/outbox integration guidance or adapters.
2. Add serializer and schema-evolution strategies for stored state and log records.
3. Add a state-machine validation model or workflow-definition DSL.
4. Add administrative recovery operations such as resume, retry, compensate, or archive.
5. Consider multi-tenant partitioning and retention policies for large-scale deployments.

---

## Suggested Test Plan Additions

The current test suite covers key happy-path mechanics, but it should be extended with failure-mode tests that match the concerns above.

### Highest-priority new tests

1. Concurrent updates to the same saga id through MongoDB reject stale writers across independent repository instances.
2. Redis persistence rejects stale writes through storage-atomic compare-and-set semantics.
3. Duplicate delivery behavior is defined and covered with deterministic tests.
4. Compensation failure produces a durable, queryable state transition.
5. Explicit assembly registration discovers external saga implementations deterministically.
6. Callback ordering is explicit and tested for both `Rejected` and any future `Compensated` notification path.

---

## Strategic Recommendation

If the goal is to keep `Genocs.Saga` lightweight, that is reasonable, but the scope should be explicit.

Two viable paths exist:

### Option A: Keep it as a lightweight orchestration core

- fix the correctness issues in Phase 1
- document the limits clearly
- move more advanced concerns into integration packages

### Option B: Evolve it into a production-ready saga platform

- complete Phases 1 through 5
- strengthen persistence contracts first
- make lifecycle, retries, idempotency, and observability first-class concepts

My recommendation is still Option A, but the emphasis should now shift from locking semantics to discovery and diagnostics. The next meaningful slice is `SAGA-016` and `SAGA-017`, because deterministic registration and startup visibility are now the most consequential gaps.