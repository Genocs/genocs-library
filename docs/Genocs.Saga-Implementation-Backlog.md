# Genocs.Saga Implementation Backlog

## Purpose

This backlog translates the architectural concerns from [docs/Genocs.Saga-Assessment.md](docs/Genocs.Saga-Assessment.md) into issue-sized work items.

The backlog is ordered for execution, not by feature area.

## Current Status

Implemented:

- `SAGA-001` Introduce durable step outcome tracking
- `SAGA-002` Exclude failed current steps from compensation replay
- `SAGA-003` Isolate per-saga execution state from shared input context
- `SAGA-004` Make terminal-state behavior explicit and safe by default
- `SAGA-005` Make in-memory state and log repositories thread-safe
- `SAGA-006` Add startup validation for saga persistence registration
- `SAGA-009` Define idempotency and duplicate-delivery handling
- `SAGA-010` Model compensation failures explicitly
- `SAGA-011` Reassess the role of the in-process keyed locker

Next recommended items:

- `SAGA-011` Reassess the role of the in-process keyed locker
- `SAGA-016` Support explicit assembly registration for saga discovery
- `SAGA-017` Add startup diagnostics for discovered sagas and bindings

## Planning Assumptions

- Focus first on correctness and operational safety.
- Avoid expanding the public API until the runtime semantics are sound.
- Prefer small, reviewable changes with tests for each task.
- Treat durable persistence behavior as the long-term source of truth, not the in-memory implementation.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Correctness and safety baseline | `SAGA-001` to `SAGA-006` |
| M2 | Persistence and concurrency hardening | `SAGA-007` to `SAGA-011` |
| M3 | Lifecycle and policy model | `SAGA-012` to `SAGA-015` |
| M4 | Diagnostics and developer tooling | `SAGA-016` to `SAGA-019` |
| M5 | Advanced platform capabilities | `SAGA-020` to `SAGA-024` |

## Execution Order

1. Finish M1 before changing integration packages.
2. Start M2 only after M1 tests are green.
3. Defer M3 public API additions until M1 and M2 semantics stabilize.
4. Build M4 in parallel with late M3 if ownership permits.
5. Treat M5 as product evolution, not stabilization work.

---

## M1: Correctness and Safety Baseline

### `SAGA-001` Introduce durable step outcome tracking

**Status**: Implemented

**Priority**: P0

**Problem**

The runtime currently logs only the message payload, which makes it impossible to distinguish a completed step from a failed step during compensation.

**Scope**

- extend the saga log model to capture outcome metadata
- add step status values such as `Completed`, `Failed`, `Compensated`
- avoid breaking consumers more than necessary

**Likely touch points**

- [src/Genocs.Saga/ISagaLogData.cs](src/Genocs.Saga/ISagaLogData.cs)
- [src/Genocs.Saga/Persistence/SagaLogData.cs](src/Genocs.Saga/Persistence/SagaLogData.cs)
- [src/Genocs.Saga/Managers/SagaProcessor.cs](src/Genocs.Saga/Managers/SagaProcessor.cs)
- [src/Genocs.Saga/Managers/SagaPostProcessor.cs](src/Genocs.Saga/Managers/SagaPostProcessor.cs)

**Acceptance criteria**

- the log record distinguishes failed and completed steps
- compensation can filter entries by outcome
- existing saga flow still compiles with updated contracts

**Dependencies**

- none

### `SAGA-002` Exclude failed current steps from compensation replay

**Status**: Implemented

**Priority**: P0

**Problem**

Compensation currently replays the failing step as if it had completed successfully.

**Scope**

- update replay logic to compensate only completed compensatable steps
- define expected behavior for failed non-compensated steps
- add tests for single-step and multi-step failure paths

**Likely touch points**

- [src/Genocs.Saga/Managers/SagaPostProcessor.cs](src/Genocs.Saga/Managers/SagaPostProcessor.cs)
- [src/tests/Genocs.Saga.UnitTests/Managers/SagaCoordinatorTests.cs](src/tests/Genocs.Saga.UnitTests/Managers/SagaCoordinatorTests.cs)

**Acceptance criteria**

- a step that throws during `HandleAsync(...)` is not compensated unless explicitly marked completed first
- reverse-order compensation still works for prior successful steps

**Dependencies**

- `SAGA-001`

### `SAGA-003` Isolate per-saga execution state from shared input context

**Status**: Implemented

**Priority**: P0

**Problem**

Parallel saga fan-out reuses one mutable `ISagaContext` instance across handlers.

**Scope**

- make input context immutable in practice or by contract
- move mutable fields like execution error data to a per-handler execution object
- ensure telemetry and callbacks read isolated execution state

**Likely touch points**

- [src/Genocs.Saga/ISagaContext.cs](src/Genocs.Saga/ISagaContext.cs)
- [src/Genocs.Saga/SagaContext.cs](src/Genocs.Saga/SagaContext.cs)
- [src/Genocs.Saga/Managers/SagaCoordinator.cs](src/Genocs.Saga/Managers/SagaCoordinator.cs)
- [src/Genocs.Saga/Managers/SagaProcessor.cs](src/Genocs.Saga/Managers/SagaProcessor.cs)

**Acceptance criteria**

- two saga handlers processing the same message cannot overwrite each other's failure state
- callbacks receive execution state for the correct saga instance
- unit tests cover parallel fan-out behavior

**Dependencies**

- none

### `SAGA-004` Make terminal-state behavior explicit and safe by default

**Status**: Implemented

**Priority**: P0

**Problem**

Completed sagas continue to process messages because only rejected sagas are blocked.

**Scope**

- define terminal-state semantics
- block completed sagas by default
- document or defer configurable reopening behavior

**Likely touch points**

- [src/Genocs.Saga/SagaProcessState.cs](src/Genocs.Saga/SagaProcessState.cs)
- [src/Genocs.Saga/Managers/SagaInitializer.cs](src/Genocs.Saga/Managers/SagaInitializer.cs)
- [src/tests/Genocs.Saga.UnitTests](src/tests/Genocs.Saga.UnitTests)

**Acceptance criteria**

- completed sagas do not process follow-up messages by default
- rejected sagas remain blocked
- tests define allowed and blocked transitions

**Dependencies**

- none

### `SAGA-005` Make in-memory state and log repositories thread-safe

**Status**: Implemented

**Priority**: P1

**Problem**

The default repositories use unsynchronized `List<T>` collections and are unsafe under parallel execution.

**Scope**

- replace list-based storage with thread-safe structures or locking
- preserve current behavior as closely as possible for tests and local development
- add concurrency-focused tests

**Likely touch points**

- [src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs](src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs)
- [src/Genocs.Saga/Persistence/InMemorySagaLog.cs](src/Genocs.Saga/Persistence/InMemorySagaLog.cs)

**Acceptance criteria**

- concurrent processing of different saga ids does not corrupt state or log storage
- test runs remain deterministic under parallel execution

**Dependencies**

- none

### `SAGA-006` Add startup validation for saga persistence registration

**Status**: Implemented

**Priority**: P1

**Problem**

`AddSaga(saga => { })` can leave the runtime with incomplete persistence registration and fail only later.

**Scope**

- validate that both `ISagaStateRepository` and `ISagaLog` are registered
- decide whether to seed in-memory defaults before applying custom configuration
- fail fast with a clear startup error

**Likely touch points**

- [src/Genocs.Saga/Extensions.cs](src/Genocs.Saga/Extensions.cs)
- [src/Genocs.Saga/Builders/SagaBuilder.cs](src/Genocs.Saga/Builders/SagaBuilder.cs)

**Acceptance criteria**

- empty callback registration fails fast or safely defaults
- partial persistence registration is rejected with a clear message
- tests cover parameterless, empty callback, and full custom registration cases

**Dependencies**

- none

---

## M2: Persistence and Concurrency Hardening

### `SAGA-007` Add versioning to saga state contracts

**Status**: Implemented

**Priority**: P1

**Problem**

The current state contract has no concurrency token or version field.

**Scope**

- extend `ISagaState` with a version or concurrency token
- update in-memory and future durable implementations to honor it

**Acceptance criteria**

- saga state includes an explicit version field
- repository implementations can detect stale writes

**Dependencies**

- `SAGA-005`

### `SAGA-008` Introduce optimistic concurrency semantics in repositories

**Status**: Implemented

**Priority**: P1

**Problem**

Distributed correctness cannot rely on the in-process locker.

**Scope**

- define conflict behavior for stale updates
- update repository contracts or persistence adapters accordingly
- add tests for concurrent updates to the same saga id

**Acceptance criteria**

- conflicting updates are detected explicitly
- the runtime can surface or handle concurrency conflicts deterministically

**Dependencies**

- `SAGA-007`

### `SAGA-009` Define idempotency and duplicate-delivery handling

**Status**: Implemented

**Priority**: P1

**Problem**

The runtime has no built-in model for duplicate message delivery.

**Scope**

- define an idempotency strategy
- decide whether deduplication belongs in the coordinator, repository, or integration layer
- introduce message identity where needed

**Acceptance criteria**

- duplicate-delivery behavior is explicitly defined and tested
- at least one baseline implementation exists for deduplication

**Dependencies**

- `SAGA-001`
- `SAGA-007`

### `SAGA-010` Model compensation failures explicitly

**Status**: Implemented

**Priority**: P1

**Problem**

Compensation is currently treated as a fire-and-forget replay phase with no dedicated durable outcome.

**Scope**

- add durable failure state for compensation errors
- decide retry vs manual intervention behavior
- add telemetry and tests for failed compensation

**Acceptance criteria**

- failed compensation is persisted and observable
- the saga does not silently disappear into a partially rolled-back state

**Dependencies**

- `SAGA-001`
- `SAGA-002`

### `SAGA-011` Reassess the role of the in-process keyed locker

**Status**: Implemented

**Priority**: P2

**Problem**

The keyed locker helps locally but can hide the need for durable concurrency guarantees.

**Scope**

- decide whether the locker remains an optimization only
- document the distributed consistency model
- ensure the runtime still behaves correctly without relying on local-only serialization

**Acceptance criteria**

- concurrency guarantees are documented clearly
- durable stores remain the correctness boundary

**Dependencies**

- `SAGA-008`

---

## M3: Lifecycle and Policy Model

### `SAGA-012` Add a lifecycle policy abstraction

**Priority**: P2

**Problem**

Terminal-state behavior is currently implicit and rigid.

**Scope**

- introduce a policy abstraction for allowed transitions
- support default terminal behavior first

**Acceptance criteria**

- lifecycle rules are explicit and testable
- reopening behavior, if supported, is policy-driven rather than accidental

**Dependencies**

- `SAGA-004`

### `SAGA-013` Add per-step execution policies

**Priority**: P2

**Problem**

There is no built-in model for retries, timeouts, or forward-only steps.

**Scope**

- define step policy primitives
- support retry count, timeout, and compensatability flags

**Acceptance criteria**

- saga steps can declare policy-driven behavior
- retry and timeout decisions are not hand-coded in every saga

**Dependencies**

- `SAGA-012`

### `SAGA-014` Expand workflow states beyond pending/completed/rejected

**Priority**: P2

**Problem**

Current states are too coarse for operational recovery.

**Scope**

- add states such as `Compensating`, `Compensated`, and `CompensationFailed`
- wire them into persistence and telemetry

**Acceptance criteria**

- runtime state communicates where a failed workflow actually stands
- operators can distinguish rejection from successful rollback

**Dependencies**

- `SAGA-010`

### `SAGA-015` Add timeout and scheduled-event support

**Priority**: P3

**Problem**

Long-running sagas need time-based behavior, not only message-driven progress.

**Scope**

- define timeout events or schedule integration points
- support expiration-based transitions

**Acceptance criteria**

- a saga can move forward or fail based on elapsed time
- timeout behavior is testable without sleeping in unit tests

**Dependencies**

- `SAGA-013`

---

## M4: Diagnostics and Developer Tooling

### `SAGA-016` Support explicit assembly registration for saga discovery

**Status**: Implemented

**Priority**: P2

**Problem**

Discovery currently depends on AppDomain load order.

**Scope**

- add assembly-targeted registration overloads
- preserve convenience discovery if desired

**Acceptance criteria**

- hosts can deterministically register saga assemblies
- external packages can be discovered without relying on incidental assembly loading

**Dependencies**

- none

### `SAGA-017` Add startup diagnostics for discovered sagas and bindings

**Status**: Implemented

**Priority**: P2

**Problem**

When discovery fails, there is no built-in diagnostic output.

**Scope**

- emit a report of discovered saga types and handled message types
- expose missing persistence or discovery problems early

**Acceptance criteria**

- startup can report what sagas were discovered and how they are bound
- misconfiguration becomes observable before first message processing

**Dependencies**

- `SAGA-016`
- `SAGA-006`

### `SAGA-018` Add runtime metrics for saga outcomes and duration

**Priority**: P3

**Problem**

Tracing exists, but counters and duration metrics are missing.

**Scope**

- emit counts for started, completed, rejected, compensated, and compensation-failed executions
- add duration metrics for processing and compensation

**Acceptance criteria**

- operators can observe volume and failure trends without trace sampling

**Dependencies**

- `SAGA-014`

### `SAGA-019` Build a deterministic saga test harness

**Priority**: P3

**Problem**

Current tests rely heavily on implementation-aware mocking rather than a reusable harness.

**Scope**

- add a first-class test harness for coordinator + repositories + fake clock + deterministic execution
- support scenario testing without ad hoc boilerplate

**Acceptance criteria**

- new saga scenarios can be tested with minimal setup
- timeout and retry features can be tested deterministically

**Dependencies**

- `SAGA-015`

---

## M5: Advanced Platform Capabilities

### `SAGA-020` Define serializer and schema-evolution strategy

**Priority**: P3

**Problem**

Persisted saga state and log payloads need a versioning story before the library is used broadly.

**Acceptance criteria**

- serialized state and log records have an evolution strategy
- contract changes have a documented migration path

**Dependencies**

- `SAGA-007`

### `SAGA-021` Add inbox/outbox integration guidance or adapters

**Priority**: P3

**Problem**

Reliable saga orchestration often depends on delivery guarantees around message publication and consumption.

**Acceptance criteria**

- the library has a recommended story for integrating with outbox/inbox patterns
- at least one reference integration path is documented or implemented

**Dependencies**

- `SAGA-009`

### `SAGA-022` Add administrative recovery operations

**Priority**: P3

**Problem**

Operators need safe recovery primitives for stuck or failed workflows.

**Status note**

The base library now includes a first recovery primitive through `ISagaCoordinator.RetryCompensationAsync<TSaga>(...)` for sagas in `CompensationFailed`. The remaining work is to widen that into a fuller operator toolset.

**Acceptance criteria**

- supported operations such as retry, resume, compensate, or archive are clearly defined
- recovery operations respect lifecycle and concurrency rules

**Dependencies**

- `SAGA-014`

### `SAGA-023` Add workflow-definition validation

**Priority**: P3

**Problem**

The current model allows accidental invalid lifecycle behavior to be encoded directly in handlers.

**Acceptance criteria**

- invalid transition patterns can be detected by validation or conventions
- the library offers guardrails for saga authoring

**Dependencies**

- `SAGA-012`

### `SAGA-024` Add multi-tenant retention and archival strategy

**Priority**: P4

**Problem**

Large-scale deployments need partitioning and cleanup policies for saga history.

**Acceptance criteria**

- retention and archival rules are explicit
- durable stores can implement cleanup without breaking recovery semantics

**Dependencies**

- `SAGA-022`

---

## Recommended First Sprint

If the goal is to start implementation immediately, this is the best first slice:

1. `SAGA-016` Support explicit assembly registration for saga discovery
2. `SAGA-017` Add startup diagnostics for discovered sagas and bindings
3. `SAGA-018` Add runtime metrics for saga outcomes and duration
4. `SAGA-022` Add administrative recovery operations

This sprint addresses the two most serious semantic defects, plus the most likely integration pitfall.

## Exit Criteria for “Production-Ready Core”

Do not market the base library as production-ready until at least these tasks are complete:

- `SAGA-001`
- `SAGA-002`
- `SAGA-003`
- `SAGA-004`
- `SAGA-005`
- `SAGA-006`
- `SAGA-007`
- `SAGA-008`
- `SAGA-009`
- `SAGA-010`

That set establishes minimum correctness, concurrency safety, and recoverability.