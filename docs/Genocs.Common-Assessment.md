# Genocs.Common — Architectural Assessment & Improvement Roadmap

**Date:** April 5, 2026  
**Reviewer:** GitHub Copilot / Software Architect  
**Version assessed:** `9.0.0-beta002` (targeting `net8.0 / net9.0 / net10.0`)  
**Package role:** Shared contracts, cross-cutting primitives, CQRS/DDD abstractions

---

## 1. Executive Summary

`Genocs.Common` has materially improved since the prior assessment. A large set of foundational concerns are now addressed:

- Paging correctness bugs are fixed (`HasPreviousPage`, out-of-range handling).
- Type constraint enforcement in `TypeList<T>` is fixed.
- Reflection property enumeration in `Extensions` is fixed.
- Domain events contract has moved to non-null, read-only semantics and now includes clearing.
- Result-returning commands, shared `Result` primitives, split event markers, specification contracts, cursor paging, outbox contracts, soft-delete filtering, and optimistic concurrency abstractions have been introduced.
- Namespace and DI-marker cleanup has progressed significantly.

That said, the package still contains a few high-impact contract and runtime risks, especially around repository sync APIs, event boundary typing, and reflection utilities for default object graph generation.

This document supersedes the previous assessment and reflects the current source state.

---

## 2. Current Findings

Issues are grouped by severity: **Critical** (correctness/runtime failure), **High** (design flaws with production impact), **Medium** (maintainability and consistency problems), and **Low** (conventions and polish).

---

### 2.1 Critical — Correctness / Runtime Failure

#### ISSUE-C01: `Extensions.TryGetDefaultValue` Can Produce Invalid Defaults for Dictionary Types

**File:** `src/Genocs.Common/Types/Extensions.cs`

`TryGetDefaultValue` has a special-case guard for type name `IDictionary\`2`, but concrete dictionary implementations are handled by the generic `IEnumerable` branch.

For example, `Dictionary<string, int>` flows into `TryGetCollectionDefaultValue`, which returns an empty array of element type, not a dictionary instance. This can result in incompatible assignment when setting properties and can throw at runtime.

**Risk:** Runtime failures when `SetDefaultInstanceProperties` is used on models containing dictionary properties.

**Recommendation:**
- Detect `IDictionary<,>` and concrete dictionary implementations via assignability checks instead of name matching.
- Return an empty dictionary instance when possible (`Activator.CreateInstance`) or skip unsupported dictionary types safely.

---

### 2.2 High — Architecture / Contract Risks

#### ISSUE-H01: `IRepositoryOfEntity` Still Exposes Broad Synchronous I/O Surface

**File:** `src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs`

Despite improvements elsewhere, the repository contract still includes many synchronous methods (`Single`, `Update`, `Delete`, `Count`, `LongCount`, etc.).

**Risk:** Thread-pool blocking in async-heavy applications and inconsistent usage patterns across services.

**Recommendation:**
- Deprecate sync methods with `[Obsolete]` in one release.
- Provide async-only contract surface in the next major.

---

#### ISSUE-H02: Event Boundary Separation Is Incomplete in Dispatcher/Event Source Contracts

**Files:**
- `src/Genocs.Common/CQRS/Events/IEventDispatcher.cs`
- `src/Genocs.Common/Domain/Entities/IAggregateRoot.cs`

`IDomainEvent` and `IIntegrationEvent` now exist, but:
- `IEventDispatcher.PublishAsync<T>` accepts `IEvent` (not `IIntegrationEvent`).
- `IGeneratesDomainEvents.DomainEvents` uses `IReadOnlyCollection<IEvent>` (not `IReadOnlyCollection<IDomainEvent>`).

**Risk:** Domain and integration event semantics can still be mixed accidentally, weakening outbox and in-process dispatch discipline.

**Recommendation:**
- Narrow `IEventDispatcher` generic constraint to `IIntegrationEvent`.
- Narrow aggregate event collections to `IDomainEvent`.

---

#### ISSUE-H03: Retrieval Throw Semantics Remain Ambiguous on `GetAsync`

**File:** `src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs`

`GetByIdAsync` now returns nullable, but `GetAsync(TKey id)` remains non-null with no explicit exception contract in XML docs.

**Risk:** Inconsistent caller expectations and silent behavioral divergence among providers.

**Recommendation:**
- Either rename to `GetRequiredAsync` / `GetOrThrowAsync` and document exception behavior,
- or make return nullable for consistency with explicit required-entity APIs elsewhere.

---

### 2.3 Medium — Maintainability / Consistency

#### ISSUE-M01: Paging Query Bounds Are Documented but Not Enforced by Base Contracts

**Files:**
- `src/Genocs.Common/CQRS/Queries/PagedQueryBase.cs`
- `src/Genocs.Common/CQRS/Queries/CursorQueryBase.cs`

Defaults are now present (`Page=0`, `Results=10`, `Limit=10`) and docs describe valid ranges, but there is no runtime guard in these base contracts.

**Risk:** Invalid values still propagate unless every consumer adds validators.

**Recommendation:**
- Add companion validation helpers in `Genocs.Common.Validation` or guarded setter patterns in base classes.

---

#### ISSUE-M02: `ISearchRequest` Still Carries Legacy Lowercase Alias in Core Contract

**File:** `src/Genocs.Common/CQRS/Queries/ISearchRequest.cs`

`SearchTerm` was introduced, but `q` remains required on the interface and is not marked obsolete.

**Risk:** Naming inconsistency continues to leak from transport concerns into shared contracts.

**Recommendation:**
- Mark `q` obsolete and remove in next major.
- Keep transport-level alias mapping at HTTP binding layer.

---

#### ISSUE-M03: Entity Equality Guidance/Contract Remains Implicit

**Files:**
- `src/Genocs.Common/Domain/Entities/IEntity.cs`
- `src/Genocs.Common/Domain/Entities/IEntityOfTPrimaryKey.cs`

`IsNew()` alias is a good improvement, but there is still no base implementation or formal guidance for identity-based equality semantics.

**Risk:** Inconsistent entity equality implementations across packages/services.

**Recommendation:**
- Add `EntityBase<TKey>` or explicit guidance contract in docs.

---

#### ISSUE-M04: `IConnectionStringSecurer` Nullability Semantics Are Still Underspecified

**File:** `src/Genocs.Common/Persistence/IConnectionStringSecurer.cs`

Namespace placement is improved (`Persistence`), but `MakeSecure` still returns nullable without a clear contract for unknown provider or malformed input behavior.

**Risk:** Defensive null handling burden on every caller.

**Recommendation:**
- Clarify XML docs and consider returning original non-null input unchanged for unsupported providers.

---

### 2.4 Low — Conventions / Polish

#### ISSUE-L01: Package Versioning Strategy Remains Confusing vs. Multi-TFM Targeting

**File:** `Directory.Build.props`

Global version remains `9.0.0-beta002` while package now targets `net10.0;net9.0;net8.0`.

**Risk:** Consumer confusion on package maturity and roadmap alignment.

**Recommendation:**
- Adopt framework-independent semantic or calendar versioning and document release policy.

---

## 3. Previously Reported Issues — Status Update

### 3.1 Resolved Since Previous Assessment

| Previous ID | Status | Evidence |
|---|---|---|
| ISSUE-01 (`HasPreviousPage`) | Resolved | `PagedResultBase.HasPreviousPage => CurrentPage > 0` |
| ISSUE-02 (`TypeList.Insert` validation) | Resolved | `TypeList.Insert` calls `CheckType(item)` |
| ISSUE-03 (`BindingFlags.Instance`) | Resolved | Uses `BindingFlags.Instance | BindingFlags.Public` |
| Security recursion depth guard | Resolved | `MaxObjectGraphDepth` and depth checks added |
| ISSUE-07/08 (domain event collection mutability/nullability + clear) | Resolved | `IReadOnlyCollection<IEvent>` + `ClearDomainEvents()` |
| ISSUE-09 (`ICommand<TResult>`) | Resolved | Added `ICommand<TResult>`, handler and dispatcher overloads |
| ISSUE-10 (`Result`, `Result<T>`, `Error`) | Resolved | Added in `src/Genocs.Common/Types/Result.cs` |
| ISSUE-11 split markers | Partially resolved | Added `IDomainEvent` and `IIntegrationEvent` |
| ISSUE-12 marker consolidation | Largely resolved | Added `IScopedDependency`; legacy interfaces marked obsolete |
| ISSUE-13 lifetime coupling in service interfaces | Resolved | `IJobService`/`INotificationSender` no longer extend transient marker |
| ISSUE-15 (`AppOptions` immutability) | Resolved | Switched to `init` setters |
| ISSUE-16 page clamping | Resolved | Constructor now throws out-of-range |
| ISSUE-18 `ServiceId` namespace | Resolved | Moved to `src/Genocs.Common/Services` |
| ISSUE-19 dead commented contract | Resolved | `IDatabaseInitializer` removed from Common |
| ISSUE-24 rejection code structure | Resolved | Introduced `RejectionCode` helper |
| ISSUE-25 nullable basic notification message | Resolved | Required + guarded setter |
| ISSUE-26 notification progress range | Resolved | Range guard added |
| ISSUE-27 `ICurrentUser.Name` docs mismatch | Resolved | XML docs corrected |
| ISSUE-28 dispatcher composition | Resolved | `IDispatcher : ICommandDispatcher, IQueryDispatcher, IEventDispatcher` |

### 3.2 Still Open or Partially Open

| Previous ID | Current State |
|---|---|
| ISSUE-04 (sync repository APIs) | Open |
| ISSUE-05 (`Get` semantics clarity) | Partially improved (`GetByIdAsync` added), still open for `GetAsync` semantics |
| ISSUE-11 (event semantic separation) | Partially open at dispatcher/source boundaries |
| ISSUE-14 (`q` naming) | Partially improved (`SearchTerm` added), still open due required `q` contract |
| ISSUE-17 (paging validation) | Partially improved (defaults/docs), still open for runtime enforcement |
| ISSUE-20 (connection string securer nullability semantics) | Partially improved (namespace move), behavior contract still open |
| ISSUE-22 (entity equality guidance) | Open |
| ISSUE-23 (versioning clarity) | Open |

---

## 4. Security Observations

| Concern | Detail |
|---|---|
| Reflection bypass | `RuntimeHelpers.GetUninitializedObject(type)` still bypasses constructor invariants; acceptable only with strict usage boundaries. |
| Private backing field mutation | Reflection write path still sets non-public backing fields for non-settable properties, which can bypass encapsulation safeguards. |
| Dictionary default handling | Collection-default logic does not safely handle dictionary implementations (see ISSUE-C01). |

**Recommendation:**
- Keep these utilities internal to controlled scenarios or add explicit API documentation warning against arbitrary external type usage.

---

## 5. Improvement Roadmap (Updated)

### Phase 1 — Correctness and Contract Safety (Immediate, 1-3 days)

| ID | Action | Risk if deferred |
|---|---|---|
| ISSUE-C01 | Fix dictionary default handling in `Extensions.TryGetDefaultValue` | Runtime assignment failures in reflection utility paths |
| ISSUE-H02 | Narrow event dispatcher and aggregate event collection types | Domain/integration event mixing remains possible |
| ISSUE-H03 | Clarify required-entity retrieval semantics (`GetAsync`) | Provider inconsistency and caller ambiguity |

### Phase 2 — Repository API Modernization (1 Sprint)

| ID | Action |
|---|---|
| ISSUE-H01 | Deprecate synchronous repository methods |
| ISSUE-H01 | Introduce async-first replacement contract guidance |
| ISSUE-M04 | Clarify and tighten `IConnectionStringSecurer` nullability contract |

### Phase 3 — Consistency Completion (1 Sprint)

| ID | Action |
|---|---|
| ISSUE-M01 | Provide baseline paging/cursor validation helpers |
| ISSUE-M02 | Mark `q` as obsolete and remove in next major |
| ISSUE-M03 | Introduce entity equality base or formalized guidance |
| ISSUE-L01 | Adopt and document clearer package versioning strategy |

### Phase 4 — Capability Completion (Future)

| Gap | Action |
|---|---|
| Multi-tenancy abstractions | Add `ITenantContext` and `ITenantInfo` contracts if still intended for `Genocs.Common` |
| Event pipeline specialization | Optionally split domain/integration dispatcher contracts for stricter layering |

---

## 6. Summary Table (Current Snapshot)

| ID | Area | Severity | Phase |
|---|---|---|---|
| ISSUE-C01 | Reflection utilities | Critical | 1 |
| ISSUE-H01 | Repository contract | High | 2 |
| ISSUE-H02 | Event contract boundaries | High | 1 |
| ISSUE-H03 | Repository retrieval semantics | High | 1 |
| ISSUE-M01 | Paging/cursor validation | Medium | 3 |
| ISSUE-M02 | Search request naming compatibility | Medium | 3 |
| ISSUE-M03 | Entity equality semantics | Medium | 3 |
| ISSUE-M04 | Connection string securer nullability contract | Medium | 2 |
| ISSUE-L01 | Package versioning clarity | Low | 3 |

---

## 7. Breaking Change Notes (Forward Plan)

The following items are potentially breaking and should follow deprecation policy:

| Change | Breaking? | Suggested strategy |
|---|---|---|
| Deprecate/remove sync repository methods | Yes | `[Obsolete]` for one major release, then remove |
| Narrow `IEventDispatcher` to integration events | Yes | Introduce new overload/interface, then deprecate old |
| Narrow aggregate domain events collection type | Yes | Add transitional adapter/property in one release |
| Remove `ISearchRequest.q` | Yes | Mark obsolete first, remove next major |

---

*Document generated by architectural reassessment of current `Genocs.Common` source, April 5, 2026.*
