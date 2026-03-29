# Genocs.Common Implementation Backlog

## Purpose

This backlog translates the architectural concerns from [docs/Genocs.Common-Assessment.md](docs/Genocs.Common-Assessment.md) into issue-sized implementation work.

The backlog is ordered for execution, not by namespace.

## Current Status

Implemented:

- M1 (COMMON-001 to COMMON-004): Complete and validated as of March 2026

Next recommended items:

- Proceed to M2 (COMMON-005 to COMMON-011) after confirming M1 adoption

## Planning Assumptions

- Fix correctness bugs before expanding the public API.
- Preserve backward compatibility where practical, but prefer clean contracts over perpetuating broken abstractions.
- Treat public contract changes as package-versioning events with explicit migration guidance.
- Pair each behavioral change with focused unit tests in the matching test project.
- Defer ecosystem-wide runtime adoption work until `Genocs.Common` contracts are stable.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Correctness and safety baseline | `COMMON-001` to `COMMON-004` |
| M2 | Domain and CQRS contract hardening | `COMMON-005` to `COMMON-011` |
| M3 | Consistency and namespace cleanup | `COMMON-012` to `COMMON-019` |
| M4 | Paging, DDD, and error-model completeness | `COMMON-020` to `COMMON-025` |
| M5 | Platform capability expansion | `COMMON-026` to `COMMON-032` |

## Execution Order

1. Finish M1 before making any public contract additions.
2. Start M2 only after M1 tests are green and the package behavior is trustworthy again.
3. Land M3 with explicit `[Obsolete]` guidance where soft migration is possible.
4. Treat M4 as quality hardening after the main contract surface is stabilized.
5. Treat M5 as opt-in product evolution, not stabilization work.

---

## M1: Correctness and Safety Baseline

### `COMMON-001` Fix zero-based previous-page detection

**Status**: Implemented & tested (March 2026)

**Priority**: P0

**Resolution**

`PagedResultBase.HasPreviousPage` now uses `CurrentPage > 0`, matching the zero-based paging contract. Regression tests verify correct behavior for first and subsequent pages.

---

### `COMMON-002` Enforce base-type validation in `TypeList<T>.Insert(...)`

**Status**: Implemented & tested (March 2026)

**Priority**: P0

**Resolution**

`TypeList<T>.Insert` now throws an `ArgumentException` if the inserted type does not match the base type. Unit tests cover both valid and invalid insertions.

---

### `COMMON-003` Repair default-instance property initialization in reflection helpers

**Status**: Implemented & tested (March 2026)

**Priority**: P0

**Resolution**

Reflection-based helpers now enumerate only public instance properties, ensuring correct property population. Tests confirm that string and reference properties are set to their default values.

---

### `COMMON-004` Add recursion and cycle safety to reflection-based default instance generation

**Status**: Implemented & tested (March 2026)

**Priority**: P0

**Resolution**

Recursion/cycle guard added to limit object graph traversal depth (max 10). Regression tests confirm that deeply nested types do not cause stack overflows and that the depth is bounded.

---

#### Test Coverage

- All fixes are covered by focused regression tests in `Genocs.Common.UnitTests`.
- Tests validate paging logic, type validation, property population, and recursion guard.

---

**M1 is complete and validated.**

**Scope**

- change `HasPreviousPage` to use `CurrentPage > 0`
- add unit tests for page `0`, page `1`, and last-page cases
- confirm consumer-facing documentation still matches runtime behavior

**Likely touch points**

- [src/Genocs.Common/CQRS/Queries/PagedResultBase.cs](src/Genocs.Common/CQRS/Queries/PagedResultBase.cs)
- [docs/Genocs.Common-Documentation.md](docs/Genocs.Common-Documentation.md)
- [docs/Genocs.Common-Human-Documentation.md](docs/Genocs.Common-Human-Documentation.md)
- matching unit tests under [src/tests](src/tests)

**Acceptance criteria**

- page `0` reports no previous page
- page `1` reports a previous page
- no current consumers need behavior-specific workarounds after the fix

**Dependencies**

- none



---

## M2: Domain and CQRS Contract Hardening

### `COMMON-005` Make domain event collections non-null and infrastructure-safe

**Status**: Implemented & documented (March 2026)

**Priority**: P1

**Resolution**

`IGeneratesDomainEvents.DomainEvents` is now a non-null, read-only collection (`IReadOnlyCollection<IEvent>`). This eliminates the need for null checks and prevents external mutation. All infrastructure and aggregate implementations must now expose domain events as a non-null, read-only collection.

---

### `COMMON-006` Add explicit domain-event clearing semantics

**Status**: Implemented & documented (March 2026)

**Priority**: P1

**Resolution**

`IGeneratesDomainEvents` now includes a `ClearDomainEvents()` method, providing a standard, explicit contract for clearing domain events after persistence and dispatch. This eliminates the need for reflection or concrete-type knowledge in infrastructure packages.

---

### `COMMON-007` Add result-returning command contracts

**Status**: Implemented & documented (March 2026)

**Priority**: P1

**Resolution**

The command model now supports result-returning contracts: `ICommand<TResult>`, `ICommandHandler<TCommand, TResult>`, and `ICommandDispatcher.SendAsync<TCommand, TResult>()` have been added. Existing void-command flow remains compatible. The dispatcher contract now clearly distinguishes between void and result-returning operations.

---

### `COMMON-008` Introduce standard success/failure result primitives

**Status**: Implemented & documented (March 2026)

**Priority**: P1

**Resolution**

Standard error and result primitives have been introduced: `Error`, `Result`, and `Result<T>`. These types provide a consistent, immutable, and serialization-friendly model for success/failure flows across the application. Usage examples are included in the code documentation.

---

### `COMMON-009` Separate domain events from integration events

**Status**: Implemented & documented (March 2026)

**Priority**: P1

**Resolution**

Introduced `IDomainEvent` and `IIntegrationEvent` marker interfaces. `IEvent` now acts as a base/bridge interface for both, ensuring backward compatibility. The package now clearly distinguishes local domain events from integration contracts, and migration guidance is provided for existing `IEvent` implementations.

---

### `COMMON-010` Redesign repository contracts for async-first usage

**Status**: Implemented & documented (March 2026)

**Priority**: P1

**Resolution**

The `IRepositoryOfEntity<TEntity, TKey>` contract is now async-first. All synchronous methods have been removed, and all async methods consistently accept a `CancellationToken`. The contract now uses `GetByIdAsync` for entity retrieval, returning `null` if not found, making not-found semantics explicit. This change ensures repository implementations are safe for modern .NET async runtimes and eliminates accidental sync-over-async issues.

**Migration Notes**

- Downstream repository implementations must remove synchronous method implementations and migrate to async-only signatures.
- Consumers should use `await repository.GetByIdAsync(id, cancellationToken)` and handle `null` results for not-found cases.
- All persistence packages must update their repository implementations to match the new async contract.

**Acceptance criteria**

- The primary repository surface is async-first.
- Optional and required read semantics are explicit.
- Migration notes exist for downstream repository implementations.

**Dependencies**

- none

### `COMMON-011` Remove provider leakage from repository query contracts

**Status**: Planned

**Priority**: P1

**Problem**

`GetAll()` and `GetAllIncluding(...)` expose `IQueryable<TEntity>` and leak ORM/provider semantics into a supposedly infrastructure-agnostic contract library.

**Scope**

- remove or obsolete direct `IQueryable<TEntity>` exposure from the domain-facing repository contract
- define the replacement direction: explicit query methods now, specification pattern later, or a separate opt-in queryable repository contract
- align with future work in `COMMON-028`

**Likely touch points**

- [src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs](src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs)
- [docs/Genocs.Common-Assessment.md](docs/Genocs.Common-Assessment.md)
- companion persistence packages after contract finalization

**Acceptance criteria**

- the default domain repository no longer requires `IQueryable` support
- consumers have a documented migration path for advanced querying

**Dependencies**

- `COMMON-010`

---

## M3: Consistency and Namespace Cleanup

### `COMMON-012` Consolidate DI lifetime markers into one namespace and model

**Status**: Implemented & documented (March 2026)

**Priority**: P2

**Resolution**

All DI lifetime marker interfaces are now consolidated under the `Genocs.Common.Dependency` namespace:

- `ISingletonDependency` (singleton)
- `ITransientDependency` (transient)
- `IScopedDependency` (scoped, new)

Obsolete duplicates:
- `IScopedService` and `ITransientService` in `Genocs.Common.Interfaces` are now marked `[Obsolete]` and inherit from the canonical marker.

**Migration Notes**
- Use only the canonical marker interfaces from `Genocs.Common.Dependency` for new code.
- Update any usages of `IScopedService` or `ITransientService` to the new canonical markers.

**Acceptance criteria**
- There is one canonical DI marker model.
- Duplicate markers are either removed or explicitly obsolete.
- Scanners can implement one predictable convention set.

**Dependencies**
- none


### `COMMON-013` Remove DI lifetime coupling from service abstractions

**Status**: Implemented & documented (March 2026)

**Priority**: P2

**Problem**

`IJobService`, `INotificationSender`, and `ISerializerService` currently embed service lifetime assumptions in contract interfaces.

**Scope**

- remove marker-interface inheritance from service abstractions where lifetime is an infrastructure concern
- verify the resulting contracts remain simple application-facing abstractions
- document registration guidance at the host or runtime package layer instead

**Likely touch points**

- [src/Genocs.Common/Interfaces/IJobService.cs](src/Genocs.Common/Interfaces/IJobService.cs)
- [src/Genocs.Common/Interfaces/INotificationSender.cs](src/Genocs.Common/Interfaces/INotificationSender.cs)
- [src/Genocs.Common/Interfaces/ISerializerService.cs](src/Genocs.Common/Interfaces/ISerializerService.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- service abstractions no longer dictate DI lifetime
- documentation explains that lifetimes belong to infrastructure registration

**Dependencies**

- `COMMON-012`

### `COMMON-014` Rename `ISearchRequest.q` to a C#-idiomatic property

**Status**: Implemented & documented (March 2026)

**Priority**: P2

**Resolution**

`ISearchRequest` and `SearchRequest` now expose `SearchTerm` as the canonical PascalCase property. A `q` alias is preserved for backward compatibility so existing consumers do not break while migrating.

**Migration Notes**

- Use `SearchTerm` for all new code and updated consumers.
- Keep HTTP query-string mapping concerns (for example `q`) in endpoint/controller binding configuration rather than domain contracts.

**Problem**

The `q` property violates C# naming conventions and pushes transport-specific query-string concerns into a domain-level contract.

**Scope**

- rename `q` to `Query` or `SearchTerm`
- update `SearchRequest`
- provide a compatibility strategy if the rename is staged across versions

**Likely touch points**

- [src/Genocs.Common/CQRS/Queries/ISearchRequest.cs](src/Genocs.Common/CQRS/Queries/ISearchRequest.cs)
- [src/Genocs.Common/CQRS/Queries/SearchRequest.cs](src/Genocs.Common/CQRS/Queries/SearchRequest.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- the public search contract uses PascalCase naming
- HTTP-specific binding concerns are documented outside the contract itself

**Dependencies**

- none

### `COMMON-015` Make `AppOptions` immutable after binding

**Status**: Implemented & documented (March 2026)

**Priority**: P2

**Resolution**

All properties on `AppOptions` are now `init`-only, making the options object immutable after binding. This ensures configuration stability and prevents accidental mutation at runtime.

**Migration Notes**

- Update any code that attempts to mutate `AppOptions` properties after construction; this will now result in a compile-time error.
- Configuration binding (e.g., via `IOptions<AppOptions>`) remains compatible with `init`-only properties in .NET 8.0+.

**Problem**

`AppOptions` uses mutable setters even though configuration should be stable after binding.

**Scope**

- change mutable `set` accessors to `init` where supported by the target frameworks
- confirm compatibility with the expected configuration binder behavior for supported TFMs
- document any required binder assumptions

**Likely touch points**

- [src/Genocs.Common/Configurations/AppOptions.cs](src/Genocs.Common/Configurations/AppOptions.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- options instances are effectively immutable after binding
- configuration examples remain accurate

**Dependencies**

- none

### `COMMON-016` Move `ServiceId` into a semantically correct namespace

**Status**: Implemented & documented (March 2026)

**Priority**: P2

**Problem**

`IServiceId` and `ServiceId` previously lived in `Genocs.Common.Builders`, which misrepresented their role and made discovery harder. They now reside in `Genocs.Common.Services`.

**Scope**

- Moved `IServiceId` and `ServiceId` to the `Genocs.Common.Services` namespace.
- Updated documentation and examples to reflect the new location.

**Likely touch points**

- [src/Genocs.Common/Services/IServiceId.cs](src/Genocs.Common/Services/IServiceId.cs)
- [src/Genocs.Common/Services/ServiceId.cs](src/Genocs.Common/Services/ServiceId.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- Service identity types live in a namespace that matches their purpose (`Genocs.Common.Services`).
- Consumers have a clear upgrade path if namespaces change (documented here).

**Dependencies**

- none

### `COMMON-017` Remove dead multi-tenant comment leakage from persistence initialization contracts

**Status**: Planned

**Priority**: P2

**Problem**

`IDatabaseInitializer` contains a commented-out tenant-specific method referencing a non-public concrete type.

**Scope**

- remove dead commented code
- decide whether multi-tenant initialization belongs in this contract family or a future dedicated abstraction

**Likely touch points**

- [src/Genocs.Common/Persistence/Initialization/IDatabaseInitializer.cs](src/Genocs.Common/Persistence/Initialization/IDatabaseInitializer.cs)
- [docs/Genocs.Common-Assessment.md](docs/Genocs.Common-Assessment.md)

**Acceptance criteria**

- no commented-out implementation leakage remains in the public contract
- future multi-tenant work has an intentional placeholder in the roadmap instead of dead code

**Dependencies**

- none

### `COMMON-018` Relocate and clarify connection-string contracts

**Status**: Implemented & documented (March 2026)

**Priority**: P2

**Problem**

Connection-string validation and masking are infrastructure concerns but previously lived under a domain namespace with weak nullability semantics. They now reside in `Genocs.Common.Persistence`.

**Scope**

- Moved the contracts to the `Genocs.Common.Persistence` namespace.
- Clarified nullability and provider-unknown behavior.
- Aligned names and docs with actual usage expectations.

**Likely touch points**

- [src/Genocs.Common/Persistence/IConnectionStringValidator.cs](src/Genocs.Common/Persistence/IConnectionStringValidator.cs)
- [src/Genocs.Common/Persistence/IConnectionStringSecurer.cs](src/Genocs.Common/Persistence/IConnectionStringSecurer.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- Connection-string contracts are not positioned as domain abstractions.
- Return-value semantics are explicitly documented and reflected in nullability annotations.

**Dependencies**

- none

### `COMMON-019` Make `IDispatcher` an explicit composite of specialized dispatchers

**Status**: Implemented & documented (March 2026)

**Priority**: P2

**Problem**

`IDispatcher` duplicated specialized dispatcher members without formally extending the specialized interfaces.

**Scope**

- Refactored `IDispatcher` to inherit from `ICommandDispatcher`, `IQueryDispatcher`, and `IEventDispatcher`.
- Ensured result-returning command methods from `COMMON-007` are represented through interface composition.
- Updated documentation to clarify intended use of the aggregate interface versus specialized interfaces.

**Likely touch points**

- [src/Genocs.Common/CQRS/Commons/IDispatcher.cs](src/Genocs.Common/CQRS/Commons/IDispatcher.cs)
- [src/Genocs.Common/CQRS/Commands/ICommandDispatcher.cs](src/Genocs.Common/CQRS/Commands/ICommandDispatcher.cs)
- [src/Genocs.Common/CQRS/Queries/IQueryDispatcher.cs](src/Genocs.Common/CQRS/Queries/IQueryDispatcher.cs)
- [src/Genocs.Common/CQRS/Events/IEventDispatcher.cs](src/Genocs.Common/CQRS/Events/IEventDispatcher.cs)

**Acceptance criteria**

- `IDispatcher` is a true aggregate abstraction, not a disconnected copy.
- Consumers can register one implementation against all intended dispatcher contracts.

**Dependencies**

- `COMMON-007`

---

## M4: Paging, DDD, and Error-Model Completeness

### `COMMON-020` Make out-of-range paging behavior explicit

**Status**: Implemented & tested (March 2026)

**Priority**: P3

**Problem**

`PagedResultBase` now throws an `ArgumentOutOfRangeException` if `currentPage` is negative or greater than or equal to `totalPages`, making caller mistakes explicit and preventing misleading metadata. This replaces the previous silent clamping behavior.

**Scope**

- Throw `ArgumentOutOfRangeException` for negative or oversized page requests in `PagedResultBase` constructor.
- Update all factory methods to use the new contract.
- Add unit tests for negative, oversized, and edge-case page requests.

**Likely touch points**

- [src/Genocs.Common/CQRS/Queries/PagedResultBase.cs](src/Genocs.Common/CQRS/Queries/PagedResultBase.cs)
- [src/Genocs.Common/CQRS/Queries/PagedResult.cs](src/Genocs.Common/CQRS/Queries/PagedResult.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- Out-of-range page handling is explicit and documented (exception thrown).
- Callers can reason about the returned page metadata without hidden clamping.
- Unit tests verify exception is thrown for invalid input.

**Dependencies**

- `COMMON-001`

### `COMMON-021` Add defaults and documented bounds for paged queries

**Status**: Planned

**Priority**: P3

**Problem**

`PagedQueryBase` allows negative pages and non-positive page sizes with no defaults or documented validation rules.

**Scope**

- assign sensible defaults for `Page` and `Results`
- document minimum and recommended maximum bounds
- keep validation-library coupling out of `Genocs.Common` while making downstream validation straightforward

**Likely touch points**

- [src/Genocs.Common/CQRS/Queries/PagedQueryBase.cs](src/Genocs.Common/CQRS/Queries/PagedQueryBase.cs)
- [src/Genocs.Common/CQRS/Queries/IPagedQuery.cs](src/Genocs.Common/CQRS/Queries/IPagedQuery.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- paged query defaults are explicit
- consumers understand the valid range and expected semantics for page inputs

**Dependencies**

- `COMMON-020`

### `COMMON-022` Clarify entity lifecycle semantics and naming

**Status**: Planned

**Priority**: P3

**Problem**

`IEntity.IsTransient()` uses legacy terminology that is easy to confuse with DI lifetime concepts.

**Scope**

- decide whether to retain `IsTransient()` with clearer docs or introduce a new preferred contract such as `IsNew()` or `IsPersisted`
- define migration guidance if a new member is introduced

**Likely touch points**

- [src/Genocs.Common/Domain/Entities/IEntity.cs](src/Genocs.Common/Domain/Entities/IEntity.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- entity lifecycle semantics are unambiguous to package consumers
- the preferred pattern is clearly documented for new code

**Dependencies**

- none

### `COMMON-023` Provide an optional entity base with identity-based equality

**Status**: Planned

**Priority**: P3

**Problem**

`IEntity<TKey>` leaves equality semantics unspecified, so consumers repeatedly reimplement entity equality and often get it wrong.

**Scope**

- add an optional `EntityBase<TKey>` or equivalent helper type
- implement identity-based equality in a way consistent with transient/new entities
- document when consumers should use the base type versus pure interfaces

**Likely touch points**

- new domain base type under [src/Genocs.Common/Domain/Entities](src/Genocs.Common/Domain/Entities)
- [docs/Genocs.Common-Human-Documentation.md](docs/Genocs.Common-Human-Documentation.md)
- matching unit tests under [src/tests](src/tests)

**Acceptance criteria**

- consumers have a canonical, reusable entity-equality implementation
- equality semantics are documented for persisted and transient entities

**Dependencies**

- `COMMON-022`

### `COMMON-024` Standardize structured rejection and error codes

**Status**: Planned

**Priority**: P3

**Problem**

`RejectedEvent.Code` is currently an unstructured string that cannot be reliably classified across services.

**Scope**

- define a structured error-code model or conventions
- align `RejectedEvent` with the shared `Error` model introduced in `COMMON-008`
- document how consumers should map these codes to HTTP, messaging, and telemetry concerns

**Likely touch points**

- [src/Genocs.Common/CQRS/Events/IRejectedEvent.cs](src/Genocs.Common/CQRS/Events/IRejectedEvent.cs)
- [src/Genocs.Common/CQRS/Events/RejectedEvent.cs](src/Genocs.Common/CQRS/Events/RejectedEvent.cs)
- result/error primitives added by `COMMON-008`

**Acceptance criteria**

- rejection and failure codes follow one documented convention
- consumers can classify errors without parsing ad hoc strings

**Dependencies**

- `COMMON-008`

### `COMMON-025` Tighten notification contract validity

**Status**: Planned

**Priority**: P3

**Problem**

Notification models allow semantically invalid instances such as empty `BasicNotification` messages and unconstrained `JobNotification.Progress` values.

**Scope**

- require meaningful notification message payloads where appropriate
- constrain job progress semantics to the documented range
- fix related documentation inconsistencies such as the `ICurrentUser.Name` summary text

**Likely touch points**

- [src/Genocs.Common/Notifications/BasicNotification.cs](src/Genocs.Common/Notifications/BasicNotification.cs)
- [src/Genocs.Common/Notifications/JobNotification.cs](src/Genocs.Common/Notifications/JobNotification.cs)
- [src/Genocs.Common/Interfaces/ICurrentUser.cs](src/Genocs.Common/Interfaces/ICurrentUser.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- notification contracts discourage or prevent invalid payloads
- XML documentation accurately describes the public surface

**Dependencies**

- none

---

## M5: Platform Capability Expansion

### `COMMON-026` Add multi-tenancy contracts

**Status**: Planned

**Priority**: P4

**Problem**

The package hints at future tenant-aware infrastructure, but it has no formal cross-cutting tenant abstractions.

**Scope**

- add `ITenantContext` and `ITenantInfo`
- keep the model transport- and persistence-neutral
- document how hosts and infrastructure packages should populate these abstractions

**Likely touch points**

- new tenant contracts under [src/Genocs.Common](src/Genocs.Common)
- documentation files under [docs](docs)

**Acceptance criteria**

- tenant identity and tenant metadata have stable shared contracts
- no concrete multitenant implementation types leak into `Genocs.Common`

**Dependencies**

- `COMMON-017`

### `COMMON-027` Add outbox and transactional event contracts

**Status**: Planned

**Priority**: P4

**Problem**

The platform has outbox-oriented packages, but `Genocs.Common` lacks a shared contract for durable event publication intent.

**Scope**

- add `IOutboxMessage` and, if justified, `IOutboxDispatcher`
- align the contract with `IIntegrationEvent`
- keep persistence and transport implementation details out of the common package

**Likely touch points**

- new outbox contracts under [src/Genocs.Common](src/Genocs.Common)
- documentation files under [docs](docs)
- future coordination with messaging and outbox packages

**Acceptance criteria**

- the outbox pattern has a package-level contract foundation
- integration-event publication intent can be modeled without binding to a concrete broker or store

**Dependencies**

- `COMMON-009`

### `COMMON-028` Add specification pattern contracts for provider-agnostic querying

**Status**: Planned

**Priority**: P4

**Problem**

The repository layer needs a provider-neutral way to express filtering, includes, ordering, and pagination once `IQueryable` leakage is removed.

**Scope**

- add `ISpecification<T>` and any minimal supporting abstractions
- keep the specification contract expressive enough for EF Core and MongoDB adapters without forcing either model
- document how repositories should consume specifications

**Likely touch points**

- new specification contracts under [src/Genocs.Common](src/Genocs.Common)
- [src/Genocs.Common/Domain/Repositories](src/Genocs.Common/Domain/Repositories)
- documentation files under [docs](docs)

**Acceptance criteria**

- consumers can express query intent without exposing `IQueryable`
- the resulting abstraction is implementable across the main Genocs persistence adapters

**Dependencies**

- `COMMON-011`

### `COMMON-029` Add cursor-based paging contracts

**Status**: Planned

**Priority**: P4

**Problem**

Offset-based paging is insufficient for high-volume and append-heavy datasets common in distributed systems.

**Scope**

- add cursor-based query and result contracts
- keep the design independent from any specific datastore cursor format
- document when cursor paging is preferable to offset paging

**Likely touch points**

- new paging contracts under [src/Genocs.Common/CQRS/Queries](src/Genocs.Common/CQRS/Queries)
- documentation files under [docs](docs)

**Acceptance criteria**

- the package supports both offset and cursor pagination models
- cursor contracts are neutral about encoding and persistence implementation

**Dependencies**

- none

### `COMMON-030` Add validation result contracts for pipeline-friendly validation

**Status**: Planned

**Priority**: P4

**Problem**

The platform relies on validation, but `Genocs.Common` has no standard validation result contract for application-layer pipelines.

**Scope**

- add `ValidationResult` and `ValidationError`
- consider whether a minimal `IValidator<T>` belongs in `Genocs.Common` or should stay in companion packages
- keep the model complementary to FluentValidation without creating a hard dependency

**Likely touch points**

- new validation contracts under [src/Genocs.Common](src/Genocs.Common)
- result/error primitives from `COMMON-008`
- documentation files under [docs](docs)

**Acceptance criteria**

- validation failures have one standard shape across the ecosystem
- no external validation library dependency is introduced in the common package

**Dependencies**

- `COMMON-008`

### `COMMON-031` Add optimistic concurrency version contracts

**Status**: Planned

**Priority**: P4

**Problem**

There is no shared contract for aggregate versioning, which complicates optimistic concurrency across persistence implementations.

**Scope**

- add a minimal versioned-entity or versioned-aggregate contract
- document how infrastructure adapters should interpret version values

**Likely touch points**

- new versioning contract under [src/Genocs.Common/Domain](src/Genocs.Common/Domain)
- documentation files under [docs](docs)

**Acceptance criteria**

- optimistic concurrency can be expressed uniformly across persistence adapters
- the version contract is minimal and transport-neutral

**Dependencies**

- `COMMON-023`

### `COMMON-032` Add soft-delete query filter contracts

**Status**: Planned

**Priority**: P4

**Problem**

`ISoftDelete` marks deleted entities but does not define how queries opt into or out of deleted-record visibility.

**Scope**

- add a minimal soft-delete filter contract for read requests
- document default visibility behavior and administrative override scenarios

**Likely touch points**

- new query/filter contracts under [src/Genocs.Common](src/Genocs.Common)
- [src/Genocs.Common/Domain/Entities/ISoftDelete.cs](src/Genocs.Common/Domain/Entities/ISoftDelete.cs)
- documentation files under [docs](docs)

**Acceptance criteria**

- consumers have a standard contract for including or excluding soft-deleted rows in queries
- the abstraction is compatible with both offset and specification-based query models

**Dependencies**

- `COMMON-028`

---

## Release Strategy Notes

- M1 should ship in the next available patch release if no public API breaks are required.
- M2 and M3 contain likely breaking contract changes and should be bundled into a planned major version.
- M4 can be split between a major release remainder and follow-up minor releases depending on API impact.
- M5 should be designed behind RFC-style proposals before code is merged, because these contracts influence multiple companion packages.

## Cross-Package Coordination

The following packages are likely affected by M2 through M5 work and should be assessed before implementation begins:

- `Genocs.Core`
- `Genocs.WebApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Persistence.MongoDB`
- `Genocs.Persistence.EFCore`
- `Genocs.Messaging`
- `Genocs.Messaging.Outbox`

## Suggested First Sprint

1. Deliver `COMMON-001` through `COMMON-004` with tests.
2. Draft the contract migration proposal for `COMMON-005` through `COMMON-011` before editing public APIs.
3. Prepare a versioning note in [CHANGELOG.md](CHANGELOG.md) once M1 is merged.
