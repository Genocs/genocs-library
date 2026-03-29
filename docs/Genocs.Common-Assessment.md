# Genocs.Common — Architectural Assessment & Improvement Roadmap

**Date:** March 28, 2026  
**Reviewer:** GitHub Copilot / Software Architect  
**Version assessed:** `9.0.0-beta002` (targeting `net8.0 / net9.0 / net10.0`)  
**Package role:** Shared contracts, cross-cutting primitives, CQRS/DDD abstractions

---

## 1. Executive Summary

`Genocs.Common` is the contract-first foundation of the Genocs ecosystem. Its intent — stable, infrastructure-agnostic abstractions for CQRS, DDD, paging, auditing, and startup conventions — is sound and a good architectural decision. However, several gaps, design inconsistencies, and bugs reduce its reliability as a shared contract library. Left unaddressed, these issues propagate into every consuming service and package.

This document catalogues all identified concerns in priority order, explains their risk, and presents a phased roadmap for resolution and enhancement.

---

## 2. Identified Issues

Issues are grouped by severity: **Critical** (correctness bugs), **High** (design flaws with real runtime impact), **Medium** (maintainability and consistency problems), and **Low** (conventions and polish).

---

### 2.1 Critical — Correctness Bugs

---

#### ISSUE-01: `HasPreviousPage` Logic Is Wrong for Zero-Based Paging

**File:** `src/Genocs.Common/CQRS/Queries/PagedResultBase.cs`

```csharp
// Current — WRONG for zero-indexed pages
public bool HasPreviousPage => CurrentPage > 1;
```

`PagedQueryBase` and its documentation explicitly state pages are **zero-indexed** (page 0 = first page). The correct guard is `CurrentPage > 0`. With the current code, a caller on page 1 (the second page) is told there is no previous page, breaking every paging navigation implementation.

**Fix:**
```csharp
public bool HasPreviousPage => CurrentPage > 0;
```

---

#### ISSUE-02: `TypeList<T>.Insert()` Bypasses Type Validation

**File:** `src/Genocs.Common/Collections/TypeList.cs`

```csharp
// CheckType is NOT called here
public void Insert(int index, Type item)
{
    _typeList.Insert(index, item);
}
```

`Add(Type item)` correctly calls `CheckType(item)`, but `Insert(int, Type)` does not. A caller can silently insert a type that violates the `TBaseType` constraint, defeating the entire purpose of the constrained collection.

**Fix:** Call `CheckType(item)` inside `Insert` before delegating to the inner list.

---

#### ISSUE-03: `Extensions.SetDefaultInstanceProperties` Uses Wrong `BindingFlags`

**File:** `src/Genocs.Common/Types/Extensions.cs`

```csharp
// BindingFlags.Instance alone returns NO properties — Public or NonPublic is required
foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance))
```

`BindingFlags.Instance` without `BindingFlags.Public` returns an empty array on every type. The method silently does nothing on all practical inputs. The correct flags are `BindingFlags.Instance | BindingFlags.Public`.

**Fix:**
```csharp
foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
```

---

### 2.2 High — Design Flaws with Runtime Impact

---

#### ISSUE-04: Synchronous I/O Methods on `IRepositoryOfEntity`

**File:** `src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs`

The repository interface declares many synchronous overloads: `Get(TKey id)`, `Single(...)`, `GetAllList()`, `Count()`, `LongCount()`, `Update(TKey, Action<TEntity>)`, `Delete(...)`, etc.

In microservice applications running on async ASP.NET Core or worker pipelines, calling these will either cause deadlocks (if the caller holds a synchronization context) or force thread-pool blocking. These methods should not be part of a modern contract library.

**Fix:** Remove all synchronous non-CancellationToken overloads from the interface. Implementations that need them for specific adapters can provide them as extension methods or concrete methods without exposing them on the contract.

---

#### ISSUE-05: `Get(TKey id)` Returns Non-Nullable `TEntity` With No Documented Throw Behavior

**File:** `src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs`

```csharp
TEntity Get(TKey id);
Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);
```

These return `TEntity`, not `TEntity?`. The implied contract is "throws if not found" — but the exception type is not documented, and the behavior cannot be enforced or inspected by callers without runtime dispatch. Compare to `FirstOrDefault(TKey id)` which correctly returns `TEntity?`.

**Fix:** Either change the return type to `TEntity?` (aligned with C# nullable annotations) or document the exact exception type and rename to `GetRequired` / `GetOrThrow` to make the throw semantics explicit.

---

#### ISSUE-06: `IRepositoryOfEntity.GetAll()` Exposes `IQueryable<TEntity>` — Leaks Persistence Abstraction

**File:** `src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs`

```csharp
IQueryable<TEntity> GetAll();
IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors);
```

Exposing `IQueryable<T>` in a domain-facing repository interface leaks the persistence model into the application and domain layers. It forces `System.Linq` as a transitive dependency and makes the interface impossible to implement without an ORM or queryable provider (e.g., MongoDB with a document provider). This violates the contract-neutrality principle the library promotes.

**Fix:** Remove `GetAll()` and `GetAllIncluding()` from `IRepositoryOfEntity`. Replace with explicitly named query methods or introduce a separate `IQueryableRepository<TEntity, TKey>` that is only implemented in ORM-backed packages.

---

#### ISSUE-07: `IGeneratesDomainEvents.DomainEvents` Is Nullable — Breaks Raise-and-Dispatch Pattern

**File:** `src/Genocs.Common/Domain/Entities/IAggregateRoot.cs`

```csharp
public interface IGeneratesDomainEvents
{
    List<IEvent>? DomainEvents { get; }
}
```

Returning a nullable mutable `List<IEvent>?` from an interface:
1. Forces every consumer to null-check before iterating.
2. A mutable list on the interface allows external callers to add or remove events directly, bypassing aggregate logic.
3. The nullable contract is meaningless on aggregates — they always have a collection (possibly empty), never null.

**Fix:**
```csharp
public interface IGeneratesDomainEvents
{
    IReadOnlyList<IEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

---

#### ISSUE-08: No `ClearDomainEvents()` Contract

**File:** `src/Genocs.Common/Domain/Entities/IAggregateRoot.cs`

After a persistence package saves and dispatches domain events, there is no standard mechanism to clear them from the aggregate. Without `ClearDomainEvents()` on the interface, infrastructure packages must use reflection or type-casting to reach the concrete list, fragmenting the ecosystem's implementation.

**Fix:** Add `void ClearDomainEvents()` to `IGeneratesDomainEvents` (see ISSUE-07).

---

#### ISSUE-09: No `ICommand<TResult>` — Commands Cannot Return Data

**File:** `src/Genocs.Common/CQRS/Commands/ICommand.cs`, `ICommandDispatcher.cs`

Only void commands are supported:
```csharp
public interface ICommandDispatcher
{
    Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand;
}
```

A common real-world requirement is a command that returns a resource identifier or status (e.g., "Create Order → return OrderId"). Without `ICommand<TResult>`, teams either abuse queries, misuse events, or build ad-hoc solutions that diverge across services.

**Fix:** Add the result-returning variant:
```csharp
public interface ICommand<TResult> : IMessage;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : class, ICommand<TResult>
{
    Task<TResult?> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
```
And extend `ICommandDispatcher` accordingly.

---

#### ISSUE-10: No `Result<T>` / `Result` Error-Handling Type

There is no standard Result/Either type for representing success-or-failure from handlers without throwing exceptions. All handler contracts return either void Tasks or nullable results. Teams will implement ad-hoc patterns (`bool`, `string? error`, output parameters) across services, breaking the contract-first consistency the library promotes.

**Fix:** Introduce a minimal, framework-free `Result` and `Result<T>` type:

```csharp
public readonly struct Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
}

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
}

public sealed record Error(string Code, string Description);
```

---

#### ISSUE-11: No Distinction Between `IDomainEvent` and `IIntegrationEvent`

**File:** `src/Genocs.Common/CQRS/Events/IEvent.cs`

`IEvent` is used for both domain events (raised inside a bounded context, dispatched synchronously in memory) and integration events (published over a broker, consumed by other services). These have fundamentally different semantics:

| Dimension | Domain Event | Integration Event |
|---|---|---|
| Scope | Within bounded context | Crosses service/context boundary |
| Transport | In-process dispatch | Broker (RabbitMQ, ASB, etc.) |
| Ordering | Determined by aggregate | Eventually consistent |
| Retry | None (typically) | Broker guarantees |

Using a single `IEvent` for both:
- Causes `MessageAttribute` to be placed on types that are actually domain-only events.
- Ambiguates outbox-pattern implementations.
- Prevents type-safe filtering in event dispatchers.

**Fix:**
```csharp
public interface IDomainEvent : IMessage;     // in-process only
public interface IIntegrationEvent : IMessage; // broker-published
```

Keep `IEvent` as an alias or base for backward compatibility during migration.

---

### 2.3 Medium — Maintainability and Consistency Problems

---

#### ISSUE-12: Duplicate Lifetime Marker Interfaces in Two Namespaces

| Interface | Namespace |
|---|---|
| `ISingletonDependency` | `Genocs.Common.Dependency` |
| `ITransientDependency` | `Genocs.Common.Dependency` |
| `IScopedService` | `Genocs.Common.Interfaces` |
| `ITransientService` | `Genocs.Common.Interfaces` |

`ITransientDependency` and `ITransientService` serve the same purpose and are discoverable by convention at the same registration priority. There is no `IScopedDependency` and no `ISingletonService`, making the set asymmetric. Scanners in companion packages (e.g., `Genocs.Core`) may only honor one set, silently ignoring the other.

**Fix:** Consolidate all lifetime markers in one namespace (`Genocs.Common.Dependency`) with a complete and symmetric set:
```
ISingletonDependency
IScopedDependency
ITransientDependency
```
Deprecate `IScopedService` and `ITransientService` with `[Obsolete]`.

---

#### ISSUE-13: `IJobService` and `INotificationSender` Extend `ITransientService`

```csharp
public interface IJobService : ITransientService
public interface INotificationSender : ITransientService
```

Embedding a DI lifetime on a domain/application contract couples the abstraction to a specific hosting convention. If an implementation needs a different lifetime (e.g., a singleton notification hub wrapper), the interface contract prevents correct registration.

**Fix:** Remove `ITransientService` from both interfaces. DI lifetime should be declared at the registration site (infrastructure layer), not on the contract.

---

#### ISSUE-14: `ISearchRequest.q` Violates C# Naming Conventions

**File:** `src/Genocs.Common/CQRS/Queries/ISearchRequest.cs`

```csharp
string q { get; set; }
```

Property names must be `PascalCase` per C# conventions and StyleCop rules. `q` is likely intended as a URL query-parameter binding convention (ASP.NET model binding), but there are cleaner ways to achieve this via `[FromQuery(Name = "q")]` at the controller/endpoint layer, keeping the domain contract clean.

**Fix:** Rename to `Query` (or `SearchTerm`) and handle query-parameter name mapping at the HTTP binding layer.

---

#### ISSUE-15: `AppOptions` Properties Use `set` Instead of `init`

**File:** `src/Genocs.Common/Configurations/AppOptions.cs`

Configuration options loaded from `appsettings.json` should be effectively immutable after binding. Using `set` allows any code to mutate them at runtime, bypassing the intended read-only nature of configuration.

**Fix:** Change all setters to `init`:
```csharp
public bool Enabled { get; init; }
public string? Name { get; init; }
```

---

#### ISSUE-16: `PagedResultBase` Silently Clamps Out-of-Range Page Requests

**File:** `src/Genocs.Common/CQRS/Queries/PagedResultBase.cs`

```csharp
CurrentPage = currentPage > totalPages ? totalPages : currentPage;
```

If a caller requests page 100 and there are only 5 pages, the result silently returns page 5. The caller has no way to detect the clamping occurred and may not realize they received the wrong page. This is a silent data correctness issue.

**Fix:** Throw `ArgumentOutOfRangeException` when `currentPage > totalPages` (if total pages are known), or return the empty result `PagedResult<T>.Empty` with the requested metadata preserved. At minimum, document this behavior clearly.

---

#### ISSUE-17: `PagedQueryBase` Has No Validation for Negative or Zero Values

**File:** `src/Genocs.Common/CQRS/Queries/PagedQueryBase.cs`

```csharp
public int Page { get; set; }    // Can be negative
public int Results { get; set; } // Can be 0 or negative
```

No guard values. A `Results` of 0 will cause a division-by-zero or database error in any concrete paging implementation. These should have default values and documented minimum bounds.

**Fix:** Add documented default values and optionally FluentValidation-compatible validators in companion packages:
```csharp
public int Page { get; set; } = 0;
public int Results { get; set; } = 20;
```
And document minimum/maximum bounds.

---

#### ISSUE-18: `ServiceId` Placed in the `Builders` Namespace — Misleading

**File:** `src/Genocs.Common/Builders/ServiceId.cs`

`IServiceId` and `ServiceId` have nothing to do with the Builder pattern. Their location in `Genocs.Common.Builders` is confusing to consumers who discover these types via IntelliSense or namespace navigation.

**Fix:** Move to `Genocs.Common.Services` or `Genocs.Common.Runtime`.

---

#### ISSUE-19: `IDatabaseInitializer` Contains Commented-Out Tenant-Scoped Method

**File:** `src/Genocs.Common/Persistence/Initialization/IDatabaseInitializer.cs`

```csharp
// Task InitializeApplicationDbForTenantAsync(GNXTenantInfo tenant, CancellationToken cancellationToken = default);
```

Dead commented-out code with a concrete type reference (`GNXTenantInfo`) leaks implementation details into a public contract. This is either unfinished multi-tenancy work or a design that was dropped.

**Fix:** Remove the comment. If multi-tenancy support is required, design it as a separate interface (`ITenantDatabaseInitializer`) when the `GNXTenantInfo` type becomes publicly available.

---

#### ISSUE-20: `IConnectionStringSecurer.MakeSecure` Returns Nullable for a Non-Nullable Input

**File:** `src/Genocs.Common/Domain/ConnectionString/IConnectionStringSecurer.cs`

```csharp
string? MakeSecure(string? connectionString, string? dbProvider = null);
```

When a valid connection string is passed in, callers have no guarantee the result is non-null. The interface does not document when null is returned, creating a trust gap. The `IConnectionStringSecurer` concept also does not belong under `Genocs.Common.Domain` — connection string infrastructure is not a domain concern.

**Fix:**
1. Move to `Genocs.Common.Infrastructure` or `Genocs.Common.Persistence` namespace.
2. Clarify nullability semantics: return the original string (possibly unsanitized label) rather than null when the provider is unknown.

---

#### ISSUE-21: `IEntity.IsTransient()` Has Unclear Semantics

**File:** `src/Genocs.Common/Domain/Entities/IEntity.cs`

`IsTransient()` is an NHibernate/ABP heritage term meaning "not yet persisted." For developers unfamiliar with this pattern, the name is ambiguous (transient could also mean "lifetime" in DI). The implementation is always `Id == Guid.Empty` or similar, but the interface does not enforce or document this.

**Fix:** Consider renaming to `IsNew()` or providing a `bool IsPersisted { get; }` property for clarity. Add XML documentation explaining the intent.

---

#### ISSUE-22: No Equality Semantics on Entity Contracts

**File:** `src/Genocs.Common/Domain/Entities/IEntityOfTPrimaryKey.cs`

DDD mandates identity-based equality for entities. `IEntity<TKey>` does not declare or encourage `IEquatable<IEntity<TKey>>`, leaving each team to implement equality heuristics independently, often incorrectly (using reference equality, which defeats entity semantics entirely).

**Fix:** Encourage (or require) equatable implementations via documentation and optionally provide a `EntityBase<TKey>` abstract class in `Genocs.Common` that implements the equality and `IsTransient()` contract, freeing consumers from boilerplate.

---

### 2.4 Low — Conventions and Polish

---

#### ISSUE-23: Version in `Directory.Build.props` Does Not Reflect .NET 10 Target

```xml
<Version>9.0.0-beta002</Version>
<!-- But targets net10.0;net9.0;net8.0 -->
```

The version `9.0.0-beta002` pre-dates the `.NET 10` target framework but the project builds for `net10.0`. This creates confusion between the package semantic version and the framework version. Use a framework-independent version scheme (e.g., calendar versioning `2026.1.0` or semantic `1.x.x`).

---

#### ISSUE-24: `RejectedEvent.Code` Is an Unstructured String

**File:** `src/Genocs.Common/CQRS/Events/RejectedEvent.cs`

```csharp
public static IRejectedEvent For(string name)
    => new RejectedEvent($"There was an error when executing: {name}", $"{name}_error");
```

The `For` factory produces codes like `"createorder_error"` which are unstructured. Callers cannot reliably switch on them or map them to HTTP status codes. There is no standardized code format or catalog.

**Fix:** Introduce a structured `ErrorCode` value object or use an `int`-based code alongside the string description, consistent with HTTP Problem Details or similar.

---

#### ISSUE-25: `BasicNotification.Message` Is Nullable Despite Being Required

**File:** `src/Genocs.Common/Notifications/BasicNotification.cs`

```csharp
public string? Message { get; set; }
```

A notification without a message is semantically meaningless. The nullable annotation and mutable setter make it easy to construct an invalid `BasicNotification`. Use a constructor-enforced required property or `init`-only property.

---

#### ISSUE-26: `JobNotification.Progress` Has No Range Guard

**File:** `src/Genocs.Common/Notifications/JobNotification.cs`

```csharp
public decimal Progress { get; set; }
```

Documentation says "between 0 and 100" but no enforcement exists. Consider range-validated initialization or a `[Range(0, 100)]` annotation for model binding validation support.

---

#### ISSUE-27: `ICurrentUser.Name` Property Description Says "user id"

**File:** `src/Genocs.Common/Interfaces/ICurrentUser.cs`

```csharp
/// <summary>
/// The user id.
/// </summary>
string? Name { get; }
```

The XML doc comment says "user id" but the property is `Name`. This is a copy-paste error in documentation that will confuse consumers.

---

#### ISSUE-28: `IDispatcher` Duplicates `ICommandDispatcher` + `IQueryDispatcher` + `IEventDispatcher` Without Clear Justification

**File:** `src/Genocs.Common/CQRS/Commons/IDispatcher.cs`

`IDispatcher` is a composition of the three specialized dispatcher interfaces but there is no formal inheritance relationship:
```csharp
// IDispatcher does NOT extend ICommandDispatcher, IQueryDispatcher, IEventDispatcher
public interface IDispatcher { ... }
```

An implementation of `IDispatcher` therefore does not automatically satisfy the three specialized contracts. A class implementing all four contracts still needs four separate registrations. The relationship should be explicit.

**Fix:**
```csharp
public interface IDispatcher : ICommandDispatcher, IQueryDispatcher, IEventDispatcher;
```

---

## 3. Security Observations

The `Extensions.cs` reflection utilities deserve specific security attention for production use:

| Concern | Detail |
|---|---|
| `RuntimeHelpers.GetUninitializedObject(type)` | Bypasses constructors; may produce objects in an internally inconsistent state if the type relies on constructor invariants. Acceptable for test utilities but risky in production data paths. |
| `GetFields(BindingFlags.NonPublic)` | Accesses private backing fields of foreign types; breaks encapsulation. A change to the target type's internals silently breaks behavior. |
| Unbounded recursion | `TryGetDefaultValue` recursively calls `SetDefaultInstanceProperties` without cycle detection. A circular reference graph in a type could cause a `StackOverflowException`. |

**Recommendation:** Add a `maxDepth` guard and document that this utility is not safe for arbitrary user-supplied types.

---

## 4. Improvement Roadmap

Issues are sequenced so that foundational correctness is addressed before new capabilities are added.

---

### Phase 1 — Critical Bugs (Immediate, 1-2 days)

| ID | Action | Risk if deferred |
|---|---|---|
| ISSUE-01 | Fix `HasPreviousPage` (`> 0` not `> 1`) | All navigable UIs show wrong state |
| ISSUE-02 | Call `CheckType` in `TypeList.Insert` | Silent type constraint violation |
| ISSUE-03 | Fix `BindingFlags` in `Extensions.cs` | `SetDefaultInstanceProperties` silently no-ops |
| Security | Add recursion depth guard in `Extensions.TryGetDefaultValue` | Potential stack overflow |

---

### Phase 2 — Domain & CQRS Contract Hardening (1 Sprint)

| ID | Action |
|---|---|
| ISSUE-07 + ISSUE-08 | Change `DomainEvents` to `IReadOnlyList<IEvent>` (non-nullable) and add `ClearDomainEvents()` |
| ISSUE-09 | Add `ICommand<TResult>` and matching handler/dispatcher overloads |
| ISSUE-10 | Introduce `Result` / `Result<T>` / `Error` primitives |
| ISSUE-11 | Split `IEvent` into `IDomainEvent` and `IIntegrationEvent` |
| ISSUE-04 | Remove synchronous methods from `IRepositoryOfEntity` (or mark `[Obsolete]`) |
| ISSUE-05 | Change `Get`/`GetAsync` return type to `TEntity?` or rename to `GetRequired` |
| ISSUE-06 | Remove `IQueryable<T>` exposure from `IRepositoryOfEntity` |

---

### Phase 3 — Consistency & Namespace Cleanup (1 Sprint)

| ID | Action |
|---|---|
| ISSUE-12 | Consolidate lifetime markers into `Genocs.Common.Dependency`; deprecate `IScopedService` / `ITransientService` |
| ISSUE-13 | Remove `ITransientService` from `IJobService` and `INotificationSender` |
| ISSUE-14 | Rename `ISearchRequest.q` to `Query` or `SearchTerm` |
| ISSUE-15 | Change `AppOptions` setters to `init` |
| ISSUE-18 | Move `ServiceId` / `IServiceId` to `Genocs.Common.Services` |
| ISSUE-19 | Remove commented-out tenant method from `IDatabaseInitializer` |
| ISSUE-20 | Move connection string interfaces out of `Genocs.Common.Domain` |
| ISSUE-28 | Make `IDispatcher` formally extend the three specialized dispatcher interfaces |

---

### Phase 4 — DDD Completeness (1 Sprint)

| ID | Action |
|---|---|
| ISSUE-16 | Document or guard `PagedResultBase` clamping behavior |
| ISSUE-17 | Add default values and document bounds on `PagedQueryBase` |
| ISSUE-21 | Improve `IsTransient()` naming and documentation |
| ISSUE-22 | Provide optional `EntityBase<TKey>` with identity-based equality |
| ISSUE-24 | Introduce structured `ErrorCode` type for `RejectedEvent` |

---

### Phase 5 — New Capabilities (Future Sprints)

These are not fixes but net-new additions that would make `Genocs.Common` a more complete cross-cutting library.

#### 5.1 Multi-Tenancy Contracts
Introduce a `ITenantContext` interface and `ITenantInfo` abstraction to formalize the multi-tenancy support implied by the commented-out `IDatabaseInitializer` method:

```csharp
public interface ITenantContext
{
    string? TenantId { get; }
    bool IsMultiTenant { get; }
}

public interface ITenantInfo
{
    string Id { get; }
    string Name { get; }
    string? DatabaseConnectionString { get; }
}
```

#### 5.2 Outbox / Transactional Event Contracts
Define an `IOutboxMessage` contract and an `IOutboxDispatcher` abstraction so the outbox pattern can be expressed at the contract level, independent of the `Genocs.Messaging.Outbox` implementation:

```csharp
public interface IOutboxMessage
{
    Guid Id { get; }
    string Type { get; }
    string Payload { get; }
    DateTime CreatedAt { get; }
    DateTime? ProcessedAt { get; }
}
```

#### 5.3 Specification Pattern Contract
Provide `ISpecification<T>` and `ISpecificationEvaluator<T>` as standard query/filter primitives:

```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
}
```

This would replace the `IQueryable<T>` leakage from `IRepositoryOfEntity` (ISSUE-06) with a structured, provider-agnostic query description.

#### 5.4 Cursor-Based Paging Contract
Add `ICursorPagedQuery` and `CursorPagedResult<T>` alongside the existing offset-based paging. Cursor paging is more consistent and performant for high-volume datasets in microservices:

```csharp
public interface ICursorPagedQuery<TCursor> : IQuery
{
    TCursor? Cursor { get; }
    int PageSize { get; }
    SortDirection Direction { get; }
}

public class CursorPagedResult<T, TCursor>
{
    public IReadOnlyList<T> Items { get; }
    public TCursor? NextCursor { get; }
    public TCursor? PreviousCursor { get; }
    public bool HasMore { get; }
}
```

#### 5.5 Validation Contract
Introduce a standard `IValidator<T>` contract (complementary to FluentValidation without hard dependency) to allow validation results to flow through the CQRS pipeline in a standard way:

```csharp
public interface IValidator<T>
{
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}

public sealed class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<ValidationError> Errors { get; }
}
```

#### 5.6 Aggregate Version / Optimistic Concurrency Contract

```csharp
public interface IVersioned
{
    long Version { get; }
}
```

Aggregates in event-sourced or optimistic-concurrency scenarios need a version. Adding this to the contracts enables persistence packages to implement concurrency checks without each package defining its own version field.

#### 5.7 Soft-Delete with Query Filter Contract
The current `ISoftDelete` only marks an entity as deleted. There is no contract for repositories to automatically filter soft-deleted records. Introduce:

```csharp
public interface ISoftDeleteFilter
{
    bool IncludeDeleted { get; }
}
```

This allows paged queries to opt-in to returning deleted records when needed (admin scenarios, audit logs).

---

## 5. Summary Table

| ID | Area | Severity | Phase |
|---|---|---|---|
| ISSUE-01 | Paging | Critical | 1 |
| ISSUE-02 | TypeList | Critical | 1 |
| ISSUE-03 | Extensions / Reflection | Critical | 1 |
| Security-01 | Extensions / Reflection | High | 1 |
| ISSUE-04 | Repository | High | 2 |
| ISSUE-05 | Repository | High | 2 |
| ISSUE-06 | Repository | High | 2 |
| ISSUE-07 | Domain Events | High | 2 |
| ISSUE-08 | Domain Events | High | 2 |
| ISSUE-09 | CQRS Commands | High | 2 |
| ISSUE-10 | CQRS Result | High | 2 |
| ISSUE-11 | Domain/Integration Events | High | 2 |
| ISSUE-12 | DI Markers | Medium | 3 |
| ISSUE-13 | DI Coupling | Medium | 3 |
| ISSUE-14 | Naming Convention | Medium | 3 |
| ISSUE-15 | Configuration | Medium | 3 |
| ISSUE-16 | Paging | Medium | 4 |
| ISSUE-17 | Paging | Medium | 4 |
| ISSUE-18 | Namespace | Medium | 3 |
| ISSUE-19 | Dead Code | Medium | 3 |
| ISSUE-20 | Namespace | Medium | 3 |
| ISSUE-21 | DDD Naming | Medium | 4 |
| ISSUE-22 | DDD Equality | Medium | 4 |
| ISSUE-23 | Versioning | Low | 3 |
| ISSUE-24 | Error Codes | Low | 4 |
| ISSUE-25 | Notifications | Low | 4 |
| ISSUE-26 | Notifications | Low | 4 |
| ISSUE-27 | Docs | Low | 1 |
| ISSUE-28 | CQRS Interfaces | Medium | 3 |

---

## 6. Migration / Breaking Change Notes

The following Phase 2 and Phase 3 changes are **breaking changes** and require a semantic version major bump or careful deprecation with `[Obsolete]`:

| Change | Breaking? | Deprecation Strategy |
|---|---|---|
| Remove sync `IRepositoryOfEntity` methods | Yes | Mark `[Obsolete]` for one major version |
| Change `DomainEvents` to `IReadOnlyList<IEvent>` | Yes | Two-step: add `ClearDomainEvents()` then change list type |
| Remove `IQueryable<T>` from repository | Yes | Provide `IQueryableRepository` as opt-in extension |
| Rename `ISearchRequest.q` | Yes | `[Obsolete]` alias property for one version |
| Deprecate `IScopedService` / `ITransientService` | Soft breaking | `[Obsolete]` redirect to unified markers |
| Remove lifetime markers from `IJobService` / `INotificationSender` | Indirect breaking | `[Obsolete]` + migration guide |

---

*Document generated by architectural review of `Genocs.Common` source, March 28, 2026.*
