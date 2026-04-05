# Genocs.Core Implementation Backlog

## Purpose

This backlog translates the Genocs.Core architectural and runtime concerns identified in the April 2026 assessment into issue-sized implementation work.

The backlog is ordered for execution, not by namespace.

## Current Status

Implemented:

- `CORE-001`: Implemented and validated with unit tests (April 2026)
- `CORE-002`: Implemented and validated with unit tests (April 2026)
- `CORE-003`: Implemented and validated with unit tests (April 2026)
- `CORE-004`: Implemented and validated with unit tests (April 2026)
- `CORE-005`: Implemented and validated with unit tests (April 2026)
- `CORE-006`: Implemented and validated with unit tests (April 2026)
- `CORE-007`: Implemented and validated with unit tests (April 2026)
- `CORE-008`: Implemented and validated with unit tests (April 2026)
- `CORE-009`: Implemented and validated with unit tests (April 2026)
- `CORE-010`: Implemented and validated with unit tests (April 2026)
- `CORE-011`: Implemented and validated with unit tests (April 2026)
- `CORE-012`: Implemented and validated with unit tests (April 2026)
- `CORE-013`: Implemented and validated with unit tests (April 2026)
- `CORE-014`: Implemented and validated with unit tests (April 2026)

Assessment baseline:

- `dotnet test src/tests/Genocs.Core.UnitTests/Genocs.Core.UnitTests.csproj -c Debug` passes with 1 test
- Genocs.Core builds with 26 warnings (nullability, obsolete contracts, analyzer/style issues)

Next recommended items:

- Continue M3 with `CORE-010` to `CORE-012` after async-first repository baseline was established in `CORE-009`

## Planning Assumptions

- Fix runtime correctness and startup safety before API expansion.
- Preserve backward compatibility where practical, but do not preserve unsafe defaults.
- Align Core implementation with stabilized Genocs.Common contracts (`COMMON-005` to `COMMON-025`).
- Pair each behavioral change with focused unit tests in Genocs.Core.UnitTests.
- Treat repository and CQRS contract changes as package-versioning events with migration notes.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Startup and dispatch correctness baseline | `CORE-001` to `CORE-004` |
| M2 | Nullability and contract alignment hardening | `CORE-005` to `CORE-008` |
| M3 | Repository and auditing model cleanup | `CORE-009` to `CORE-012` |
| M4 | Security and diagnostics hardening | `CORE-013` to `CORE-015` |
| M5 | Test and platform maturity expansion | `CORE-016` to `CORE-018` |

## Execution Order

1. Complete M1 before broadening handler registration or repository APIs.
2. Start M2 only after M1 tests are green and startup behavior is deterministic.
3. Land M3 with migration guidance for persistence adapters.
4. Use M4 to reduce operational/security risk after core semantics stabilize.
5. Treat M5 as quality and ecosystem expansion, not first-pass stabilization.

---

## M1: Startup and Dispatch Correctness Baseline

### `CORE-001` Remove sync-over-async startup execution in `UseGenocs()`

**Status**: Implemented & tested (April 2026)

**Priority**: P0

**Resolution**

Added `UseGenocsAsync(...)` as the direct async startup path and changed `UseGenocs()` to delegate to it without using `Task.Run(...)`. This preserves the existing synchronous entry point while eliminating the extra task hop and ensuring cancellation is explicitly propagated in the async flow.

Unit tests now verify that:

- cancellation tokens are passed through to `IStartupInitializer.InitializeAsync(...)`
- initializer exceptions are rethrown by `UseGenocs()`

**Problem**

`UseGenocs()` used `Task.Run(...).GetAwaiter().GetResult()`, which introduced sync-over-async startup behavior and could mask cancellation/shutdown semantics.

**Scope**

- replace sync-over-async execution with a direct async startup flow
- preserve the existing public extension entry point while enabling cancellation-aware initialization
- document startup ordering and failure behavior

**Likely touch points**

- [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)
- [src/Genocs.Core/Builders/StartupInitializer.cs](src/Genocs.Core/Builders/StartupInitializer.cs)
- [docs/Genocs.Core-Human-Documentation.md](docs/Genocs.Core-Human-Documentation.md)

**Acceptance criteria**

- startup initialization executes without `Task.Run(...).GetAwaiter().GetResult()`
- initializer exceptions are surfaced predictably
- cancellation is propagated through the initializer pipeline

**Dependencies**

- none

### `CORE-002` Correct and unify default endpoint root response behavior

**Status**: Implemented & tested (April 2026)

**Priority**: P0

**Resolution**

Both `MapDefaultEndpoints` overloads now share identical root-response logic: write the configured service name if present, otherwise write a `"Service <version> is running"` message. The double `GetService<AppOptions>()` call in the `WebApplication` overload and the dead `message` variable in the `IApplicationBuilder` overload are both removed.

Unit tests cover three root-response scenarios (name configured, name absent, no options registered) and the `/healthz` and `/alive` health endpoints. The test host uses the modern `IHostBuilder.UseTestServer()` path.

**Problem**

`MapDefaultEndpoints` had inconsistent behavior across overloads. The `IApplicationBuilder` overload computed a `message` value that was never used and fell back to a hard-coded `"Service"` literal. The `WebApplication` overload called `GetService<AppOptions>()` twice unnecessarily.

**Scope**

- unify root endpoint response semantics between `IApplicationBuilder` and `WebApplication` overloads
- remove dead/unused local variables
- explicitly document production exposure expectations for health and root endpoints

**Likely touch points**

- [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)
- [docs/Genocs.Core-Human-Documentation.md](docs/Genocs.Core-Human-Documentation.md)
- [docs/Genocs.Core-Agent-Documentation.md](docs/Genocs.Core-Agent-Documentation.md)

**Acceptance criteria**

- both overloads produce consistent root-response behavior for equivalent configuration
- no dead code remains in root endpoint mapping
- documentation clearly states development-only vs always-mapped behavior

**Dependencies**

- none

### `CORE-003` Replace reflection-based query dispatch invocation with typed dispatch path

**Status**: Implemented & tested (April 2026)

**Resolution**

Replaced `MakeGenericType` + `MethodInfo.Invoke` with a `ConcurrentDictionary<(Type Query, Type Result), IQueryHandlerInvoker>` cache. A private `QueryHandlerInvoker<TQuery, TResult>` implementation is created once per distinct query/result type pair via `Activator.CreateInstance`; subsequent calls dispatch through a virtual method with no reflection. Both `QueryAsync` overloads now use `CreateAsyncScope`. Null arguments are guarded with `ArgumentNullException.ThrowIfNull`.

Unit tests verify typed dispatch, polymorphic dispatch, null-guard behaviour, missing-handler failure, and `IDispatcher` facade delegation.

**Original Priority**: P0

**Priority**: P0

**Problem**

`QueryDispatcher.QueryAsync<TResult>(IQuery<TResult> query, ...)` resolves handlers via reflection (`MakeGenericType` + `MethodInfo.Invoke`), creating runtime fragility and nullability warnings.

**Scope**

- replace reflection invocation with a typed, cached, or otherwise safe dispatch mechanism
- preserve support for runtime query instances when only non-generic entry points are available
- improve exception messages for missing handlers

**Likely touch points**

- [src/Genocs.Core/CQRS/Queries/Dispatchers/QueryDispatcher.cs](src/Genocs.Core/CQRS/Queries/Dispatchers/QueryDispatcher.cs)
- [src/Genocs.Core/CQRS/Commons/InMemoryDispatcher.cs](src/Genocs.Core/CQRS/Commons/InMemoryDispatcher.cs)
- matching tests under [src/tests/Genocs.Core.UnitTests](src/tests/Genocs.Core.UnitTests)

**Acceptance criteria**

- query dispatch does not rely on `MethodInfo.Invoke` for primary execution path
- missing-handler failures are explicit and actionable
- nullability warnings in query dispatcher are eliminated

**Dependencies**

- none

### `CORE-004` Make handler registration deterministic and explicit

**Status**: Implemented & tested (April 2026)

**Resolution**

Unified handler registration through a shared `HandlerRegistration` helper used by `AddHandlers(project)`, `AddCommandHandlers`, `AddEventHandlers`, and `AddQueryHandlers`. The helper now provides deterministic assembly ordering (`FullName` ordinal sort), optional project filtering as explicit opt-in behavior, and duplicate-safe registration (`RegistrationStrategy.Skip`).

All command/query/event handlers now register with the same transient lifetime across all entry points. Unit tests validate project-filter behavior, duplicate prevention on repeated registration calls, and lifetime consistency for both `IServiceCollection` and `IGenocsBuilder` registration paths.

**Original Priority**: P0

**Priority**: P0

**Problem**

Genocs.Core exposes multiple registration paths with inconsistent scope/lifetime and scanning behavior (`AddHandlers(project)` uses scoped + name filtering, builder extension paths use transient + broad AppDomain scanning).

**Scope**

- define one canonical registration model and lifetime guidance
- keep optional project/assembly filtering as explicit opt-in behavior
- document registration precedence and duplicate handler risks

**Likely touch points**

- [src/Genocs.Core/CQRS/Commons/Extensions.cs](src/Genocs.Core/CQRS/Commons/Extensions.cs)
- [src/Genocs.Core/CQRS/Commands/Extensions.cs](src/Genocs.Core/CQRS/Commands/Extensions.cs)
- [src/Genocs.Core/CQRS/Queries/Extensions.cs](src/Genocs.Core/CQRS/Queries/Extensions.cs)
- [src/Genocs.Core/CQRS/Events/Extensions.cs](src/Genocs.Core/CQRS/Events/Extensions.cs)

**Acceptance criteria**

- one recommended registration path is documented and test-covered
- lifetime choice is intentional and consistent across command/query/event handlers
- assembly scanning behavior is predictable and diagnosable

**Dependencies**

- none

---

## M2: Nullability and Contract Alignment Hardening

### `CORE-005` Make aggregate domain-event collections non-null by contract

**Status**: Implemented & tested (April 2026)

**Resolution**

Updated `AggregateRoot<TPrimaryKey>` so `DomainEvents` is initialized inline as a non-null list (`List<IEvent> = []`) and `ClearDomainEvents()` now clears the collection directly without null checks. This aligns runtime behavior with the `IGeneratesDomainEvents` contract (`Never null; always a read-only collection` via interface projection).

Added unit tests for initialization, explicit-interface projection, and clear semantics to ensure the collection remains non-null and behaviorally stable.

**Original Priority**: P1

**Priority**: P1

**Problem**

`AggregateRoot<TPrimaryKey>.DomainEvents` is declared as nullable while the explicit interface projection is non-null, producing warnings and weakening invariants.

**Scope**

- make aggregate event collection non-null and read-only to callers where practical
- keep mutation/clear semantics explicit for aggregate internals
- align with Genocs.Common domain-event contract assumptions

**Likely touch points**

- [src/Genocs.Core/Domain/Entities/AggregateRoot.cs](src/Genocs.Core/Domain/Entities/AggregateRoot.cs)
- [docs/Genocs.Core-Human-Documentation.md](docs/Genocs.Core-Human-Documentation.md)

**Acceptance criteria**

- no nullable warning remains for domain-event collection exposure
- infrastructure code no longer needs null guards for domain events
- clear semantics remain explicit (`ClearDomainEvents`)

**Dependencies**

- `COMMON-005`
- `COMMON-006`

### `CORE-006` Fix `EntityNotFoundException` nullability and construction invariants

**Status**: Implemented & tested (April 2026)

**Resolution**

Enforced constructor invariants so `EntityType` and `Id` are always initialized across all construction paths:

- type-based constructors now accept nullable `id` and normalize null to a stable sentinel (`"<unknown>"`)
- message-based constructors initialize `EntityType` to `typeof(object)` and `Id` to `"<unknown>"`
- type-based constructors guard `entityType` with `ArgumentNullException.ThrowIfNull`

This removes the uninitialized-property warnings in `EntityNotFoundException` and avoids nullability pressure at repository call sites passing potentially nullable keys.

Added constructor-focused unit tests to validate all overloads (including inner exceptions) produce consistent state.

**Original Priority**: P1

**Priority**: P1

**Problem**

String-only constructors do not initialize `EntityType` and `Id`, producing non-nullability warnings and inconsistent exception state.

**Scope**

- enforce consistent initialization strategy for all constructors
- decide whether free-form message constructors should remain public
- ensure serialization-safe and logging-safe property values

**Likely touch points**

- [src/Genocs.Core/Domain/Entities/EntityNotFoundException.cs](src/Genocs.Core/Domain/Entities/EntityNotFoundException.cs)

**Acceptance criteria**

- all constructors produce internally consistent exception state
- nullability warnings for `EntityType` and `Id` are resolved

**Dependencies**

- none

### `CORE-007` Resolve auditing nullability mismatches against `Genocs.Common` interfaces

**Status**: Implemented & tested (April 2026)

**Resolution**

Aligned `CreatorUser` nullability in Core audited models with `Genocs.Common` auditing contracts by changing creator navigation properties from nullable to non-nullable (`TUser`) in the four generic base types:

- `CreationAuditedEntity<TPrimaryKey, TUser>`
- `AuditedEntity<TPrimaryKey, TUser>`
- `CreationAuditedAggregateRoot<TPrimaryKey, TUser>`
- `AuditedAggregateRoot<TPrimaryKey, TUser>`

Each property is now initialized with `default!` to preserve model-binding/ORM materialization behavior while satisfying interface nullability.

Added focused unit tests that validate creator-user assignment and projection through `ICreationAudited<TUser>` and `IAudited<TUser>` across both entity and aggregate-root hierarchies.

**Original Priority**: P1

**Priority**: P1

**Problem**

Audited entity/aggregate implementations expose nullable user navigation properties where implemented interfaces require non-nullable members.

**Scope**

- align Core audited models with current interface nullability contracts
- if needed, refine interface contracts in coordinated Genocs.Common change proposal
- add focused tests for audited model compatibility

**Likely touch points**

- [src/Genocs.Core/Domain/Entities/Auditing/AuditedEntity.cs](src/Genocs.Core/Domain/Entities/Auditing/AuditedEntity.cs)
- [src/Genocs.Core/Domain/Entities/Auditing/CreationAuditedEntity.cs](src/Genocs.Core/Domain/Entities/Auditing/CreationAuditedEntity.cs)
- [src/Genocs.Core/Domain/Entities/Auditing/AuditedAggregateRoot.cs](src/Genocs.Core/Domain/Entities/Auditing/AuditedAggregateRoot.cs)
- [src/Genocs.Core/Domain/Entities/Auditing/CreationAuditedAggregateRoot.cs](src/Genocs.Core/Domain/Entities/Auditing/CreationAuditedAggregateRoot.cs)

**Acceptance criteria**

- no CS8766 nullability mismatch warnings remain in auditing models
- audited entity contracts are explicit about required vs optional navigation references

**Dependencies**

- none

### `CORE-008` Remove obsolete DI marker usage from Core contracts

**Status**: Implemented & tested (April 2026)

**Resolution**

Replaced obsolete `ITransientService` inheritance with the canonical `ITransientDependency` marker from `Genocs.Common.Dependency` in the remaining Core contracts:

- `IDapperRepository`
- `IAuditService`

Updated namespaces accordingly and validated the change with the Core unit test suite and source scan (no remaining `ITransientService` usage in `src/Genocs.Core`).

**Original Priority**: P1

**Priority**: P1

**Problem**

Core contracts still reference obsolete markers (for example `ITransientService`) despite Genocs.Common DI-marker consolidation.

**Scope**

- replace obsolete marker inheritance with canonical dependency markers
- align namespace and guidance with Genocs.Common dependency contracts
- document migration impact for downstream implementations

**Likely touch points**

- [src/Genocs.Core/Domain/Repositories/IDapperRepository.cs](src/Genocs.Core/Domain/Repositories/IDapperRepository.cs)
- [src/Genocs.Core/Domain/Entities/Auditing/IAuditService.cs](src/Genocs.Core/Domain/Entities/Auditing/IAuditService.cs)
- [docs/Genocs.Core-Human-Documentation.md](docs/Genocs.Core-Human-Documentation.md)

**Acceptance criteria**

- no CS0618 warnings for obsolete DI marker usage in Genocs.Core
- Core contracts follow canonical Genocs.Common DI marker conventions

**Dependencies**

- `COMMON-012`
- `COMMON-013`

---

## M3: Repository and Auditing Model Cleanup

### `CORE-009` Plan and execute async-first repository migration in Core

**Status**: Implemented & tested (April 2026)

**Resolution**

Implemented Phase 1 of the async-first migration as a non-breaking foundation and documented the versioned migration path.

Code changes:

- Added async-first override hooks in `RepositoryBase<TEntity, TKey>` and routed public async methods through them:
	- `GetAllListCoreAsync`
	- `FirstOrDefaultByIdCoreAsync`
	- `FirstOrDefaultCoreAsync`
	- `SingleCoreAsync`
	- `CountCoreAsync`
	- `LongCountCoreAsync`
- Added `GetByIdAsync` to align Core base behavior with `Genocs.Common` repository expectations.
- Preserved existing sync methods and signatures for compatibility.

Documentation:

- Added phased migration and versioning guidance in [docs/Genocs.Core-Repository-Migration-Plan.md](docs/Genocs.Core-Repository-Migration-Plan.md).

Validation:

- Added unit tests to verify async public methods flow through async-first hooks in [src/tests/Genocs.Core.UnitTests/Domain/Repositories/RepositoryBaseAsyncFlowTests.cs](src/tests/Genocs.Core.UnitTests/Domain/Repositories/RepositoryBaseAsyncFlowTests.cs).

**Original Priority**: P2

**Priority**: P2

**Problem**

`RepositoryBase<TEntity, TKey>` still exposes synchronous and `IQueryable`-centric patterns that conflict with Genocs.Common async-first and provider-agnostic direction.

**Scope**

- define phased migration from sync APIs to async-first APIs
- reduce provider leakage from default domain-facing repository abstractions
- coordinate migration with persistence adapters and specs support

**Likely touch points**

- [src/Genocs.Core/Domain/Repositories/RepositoryBase.cs](src/Genocs.Core/Domain/Repositories/RepositoryBase.cs)
- [src/Genocs.Core/Domain/Repositories/IRepository.cs](src/Genocs.Core/Domain/Repositories/IRepository.cs)
- companion persistence packages

**Acceptance criteria**

- migration plan and versioning path are documented
- default repository flow is async-first for new implementations
- provider-specific querying remains opt-in rather than baseline contract

**Dependencies**

- `COMMON-010`
- `COMMON-011`
- `COMMON-028`

### `CORE-010` Restore deterministic creation-audit behavior

**Status**: Implemented & tested (April 2026)

**Resolution**

Restored deterministic audit timestamp behavior and aligned it to UTC semantics:

- `EntityAuditingHelper.SetCreationAuditProperties` now sets `CreatedAt` when missing
- `EntityAuditingHelper.SetModificationAuditProperties` now sets `LastUpdate` using UTC
- `CreationAuditedEntity<TPrimaryKey>` and `CreationAuditedAggregateRoot<TPrimaryKey>` constructors now initialize `CreatedAt` with `DateTime.UtcNow`

Also simplified the helper’s null-safety flow and preserved the existing behavior when `userId` is unavailable (creation/modification user IDs are not force-set).

Added focused unit tests for creation and modification flows, including:

- set-when-missing behavior for `CreatedAt`
- no override when `CreatedAt` is already present
- UTC-kind assertions for both creation and modification timestamps
- unknown-user behavior for creator/updater IDs

**Original Priority**: P2

**Priority**: P2

**Problem**

`EntityAuditingHelper.SetCreationAuditProperties` currently leaves creation time assignment commented out, which can leave `CreatedAt` unset despite audited interfaces.

**Scope**

- restore creation timestamp behavior with UTC semantics
- ensure both creation and modification helpers use consistent time source strategy
- define behavior when user id is unavailable

**Likely touch points**

- [src/Genocs.Core/Domain/Entities/Auditing/EntityAuditingHelper.cs](src/Genocs.Core/Domain/Entities/Auditing/EntityAuditingHelper.cs)
- [src/Genocs.Core/Domain/Entities/Auditing/CreationAuditedEntity.cs](src/Genocs.Core/Domain/Entities/Auditing/CreationAuditedEntity.cs)

**Acceptance criteria**

- creation time is consistently set when absent
- UTC/local-time behavior is documented and test-validated
- nullability warnings around audit helper logic are resolved

**Dependencies**

- none

### `CORE-011` Eliminate dead commented multi-tenancy blocks from Core runtime helpers

**Status**: Implemented & tested (April 2026)

**Priority**: P2

**Resolution**

Removed legacy commented multi-tenancy and infrastructure scaffolding from `RepositoryBase<TEntity, TKey>`, including dead `using` directives, dormant interface fragments, and an empty static constructor that only contained commented code.

Added explicit documentation guidance that multi-tenancy behavior is intentionally owned by companion packages, not by commented placeholders in Core runtime helpers.

**Problem**

Core contains large commented multi-tenancy and legacy infrastructure blocks in runtime classes, increasing maintenance overhead and masking intended behavior.

**Scope**

- remove dead commented code from repository and auditing helpers
- replace with concise architectural notes in docs if behavior is intentionally deferred
- keep multi-tenancy concerns in dedicated packages

**Likely touch points**

- [src/Genocs.Core/Domain/Repositories/RepositoryBase.cs](src/Genocs.Core/Domain/Repositories/RepositoryBase.cs)
- [src/Genocs.Core/Domain/Entities/Auditing/EntityAuditingHelper.cs](src/Genocs.Core/Domain/Entities/Auditing/EntityAuditingHelper.cs)
- [docs/Genocs.Core-Human-Documentation.md](docs/Genocs.Core-Human-Documentation.md)

**Acceptance criteria**

- dead commented runtime logic is removed
- deferred architectural concerns are documented in docs, not inline dead blocks

**Dependencies**

- none

### `CORE-012` Improve startup option resolution without ad hoc service-provider creation

**Status**: Implemented & tested (April 2026)

**Priority**: P2

**Resolution**

Made configuration resolution deterministic for `AddGenocs(IServiceCollection, IConfiguration?)` by selecting one source-of-truth at builder creation time:

- explicit `IConfiguration` argument when provided
- otherwise, an existing `IConfiguration` singleton instance already registered in `IServiceCollection`
- otherwise, an empty configuration root (`new ConfigurationBuilder().Build()`)

`GetOptions<TModel>(IGenocsBuilder, sectionName)` now reads only `builder.Configuration` and no longer creates a temporary service provider.

Added focused unit tests for explicit-configuration, pre-registered configuration, and no-configuration fallback paths.

**Problem**

`IGenocsBuilder.GetOptions<TModel>` builds a temporary service provider when `Configuration` is null, which can duplicate singletons and produce startup ambiguity.

**Scope**

- avoid temporary provider construction for options retrieval
- define one explicit source-of-truth strategy for configuration access
- keep behavior compatible for non-web hosts using `IServiceCollection`

**Likely touch points**

- [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)
- [src/Genocs.Core/Builders/GenocsBuilder.cs](src/Genocs.Core/Builders/GenocsBuilder.cs)

**Acceptance criteria**

- options retrieval does not require `BuildServiceProvider()` in helper paths
- startup configuration behavior is deterministic and documented

**Dependencies**

- none

---

## M4: Security and Diagnostics Hardening

### `CORE-013` Harden RSA XML helper failure model and input handling

**Status**: Implemented & tested (April 2026)

**Priority**: P3

**Resolution**

Hardened `Encryption.FromXmlFile` and `Encryption.ToXmlFile` with explicit input guards and structured RSA XML validation.

Runtime behavior changes:

- argument checks now throw `ArgumentNullException` / `ArgumentException` for invalid inputs
- missing files throw `FileNotFoundException`
- malformed XML, invalid root element, missing required nodes, and invalid base64 node values throw `FormatException`
- cryptographic import failures throw `CryptographicException` with contextual messaging

Added focused unit tests covering malformed XML and incomplete RSA key documents, plus successful valid-key import.

**Problem**

`Encryption.FromXmlFile` throws `Exception` for invalid XML keys and performs minimal structural validation.

**Scope**

- replace generic exception throws with specific argument/format/cryptographic exceptions
- validate XML structure and required RSA nodes before import
- document whether legacy XML key format remains supported long-term

**Likely touch points**

- [src/Genocs.Core/Extensions/Encryption.cs](src/Genocs.Core/Extensions/Encryption.cs)
- [src/tests/Genocs.Core.UnitTests](src/tests/Genocs.Core.UnitTests)

**Acceptance criteria**

- invalid key documents produce specific, actionable exception types
- XML parsing path is validated with malformed and incomplete key tests

**Dependencies**

- none

### `CORE-014` Add startup/CQRS registration diagnostics hooks

**Status**: Implemented & tested (April 2026)

**Priority**: P3

**Resolution**

Added an opt-in diagnostics module for Core startup and CQRS registration paths:

- introduced `AddCoreDiagnostics(...)` for both `IGenocsBuilder` and `IServiceCollection`
- added `CoreDiagnosticsState` and `CoreDiagnosticsOptions` to collect lightweight runtime diagnostics
- instrumented handler scanning to report discovered handler candidates
- added warning-level diagnostics when dispatchers are registered without corresponding handler registrations
- instrumented startup initializer registration/execution flow to surface registered initializers and execution counts

Added focused unit tests validating empty-handler warning signals and startup initializer diagnostics visibility.

**Problem**

When handler scanning or initializer registration misconfigures, Core currently provides little diagnostic visibility.

**Scope**

- add optional diagnostics around discovered handlers and registered initializers
- include warning-level signals for empty handler sets when dispatchers are registered
- keep diagnostics opt-in and lightweight for production workloads

**Likely touch points**

- [src/Genocs.Core/CQRS/Commons/Extensions.cs](src/Genocs.Core/CQRS/Commons/Extensions.cs)
- [src/Genocs.Core/Builders/GenocsBuilder.cs](src/Genocs.Core/Builders/GenocsBuilder.cs)
- [src/Genocs.Core/Builders/StartupInitializer.cs](src/Genocs.Core/Builders/StartupInitializer.cs)

**Acceptance criteria**

- developers can quickly verify which handlers/initializers were registered
- common startup misconfigurations are observable without debugger-only inspection

**Dependencies**

- `CORE-004`

### `CORE-015` Normalize analyzer and nullability baseline for Core

**Status**: Planned

**Priority**: P3

**Problem**

Current Core build emits multiple warnings (nullability and StyleCop), reducing signal-to-noise and masking regressions.

**Scope**

- resolve current warning set in Core project files
- codify warning baseline policy for CI (no new warnings)
- add tests where warnings expose behavioral uncertainty

**Likely touch points**

- [src/Genocs.Core](src/Genocs.Core)
- [Directory.Build.props](Directory.Build.props)
- [Directory.Build.targets](Directory.Build.targets)

**Acceptance criteria**

- Core builds warning-clean or with explicitly documented accepted exceptions
- CI policy prevents warning regression for touched Core files

**Dependencies**

- `CORE-005`
- `CORE-006`
- `CORE-007`
- `CORE-010`
- `CORE-013`

---

## M5: Test and Platform Maturity Expansion

### `CORE-016` Expand Genocs.Core unit-test coverage beyond encryption hash helper

**Status**: Planned

**Priority**: P4

**Problem**

Current unit coverage is effectively limited to one MD5 extension test, leaving core runtime pathways unverified.

**Scope**

- add unit tests for startup initializer sequencing and failure behavior
- add tests for command/query/event dispatch success and missing-handler paths
- add tests for entity equality/transient semantics and domain-event collection behavior
- add tests for default endpoint mapping behavior

**Likely touch points**

- [src/tests/Genocs.Core.UnitTests](src/tests/Genocs.Core.UnitTests)
- [src/Genocs.Core](src/Genocs.Core)

**Acceptance criteria**

- Core tests cover startup, dispatching, entity semantics, and endpoint behavior
- regression tests exist for each M1 and M2 behavior change

**Dependencies**

- `CORE-001`
- `CORE-002`
- `CORE-003`
- `CORE-005`

### `CORE-017` Add integration tests for DI scanning and dispatcher wiring

**Status**: Planned

**Priority**: P4

**Problem**

Runtime behavior depends on assembly scanning and DI wiring that are not exercised by current tests.

**Scope**

- add integration tests that host a minimal service collection/web host
- validate handler discovery, dispatcher registration, and initializer execution order
- test both explicit project-filter scanning and full AppDomain scanning modes

**Likely touch points**

- new integration tests under [src/tests](src/tests)
- [src/Genocs.Core/CQRS/Commons/Extensions.cs](src/Genocs.Core/CQRS/Commons/Extensions.cs)
- [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)

**Acceptance criteria**

- integration tests fail on handler registration regressions
- startup and dispatch wiring behaviors are validated end-to-end

**Dependencies**

- `CORE-004`
- `CORE-014`

### `CORE-018` Define Core vNext package boundary and deprecation roadmap

**Status**: Planned

**Priority**: P4

**Problem**

Genocs.Core currently mixes foundational runtime concerns, legacy repository base behavior, and utility helpers without an explicit deprecation and extraction roadmap.

**Scope**

- define which APIs remain Core baseline vs move to companion packages
- publish deprecation guidance for legacy/overlapping abstractions
- align roadmap with Genocs.Common and persistence package evolution

**Likely touch points**

- [docs/Genocs.Core-Human-Documentation.md](docs/Genocs.Core-Human-Documentation.md)
- [docs/Genocs.Core-Agent-Documentation.md](docs/Genocs.Core-Agent-Documentation.md)
- [CHANGELOG.md](CHANGELOG.md)

**Acceptance criteria**

- Core vNext boundary is documented with concrete keep/deprecate decisions
- deprecation timeline exists for impacted APIs
- companion package coordination points are explicit

**Dependencies**

- `CORE-009`
- `CORE-015`

---

## Release Strategy Notes

- M1 should ship first in a stabilization release (patch/minor depending on API impact).
- M2 and M3 likely include contract-facing changes and should be grouped into a planned major-version stream if breaking.
- M4 can be delivered incrementally after runtime contracts stabilize.
- M5 should be parallelized with ongoing package roadmap and CI quality improvements.

## Cross-Package Coordination

The following packages are likely affected by M2 through M5 work and should be reviewed before implementation begins:

- `Genocs.Common`
- `Genocs.WebApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Persistence.MongoDB`
- `Genocs.Persistence.EFCore`
- `Genocs.Messaging`

## Suggested First Sprint

1. Deliver `CORE-001` through `CORE-004` with unit tests.
2. Fix nullability and obsolete contract warnings via `CORE-005` through `CORE-008`.
3. Prepare migration notes for repository changes planned in `CORE-009`.
4. Update [CHANGELOG.md](CHANGELOG.md) with the M1 stabilization notes after merge.
