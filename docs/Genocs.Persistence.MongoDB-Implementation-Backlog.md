# Genocs.Persistence.MongoDB Implementation Backlog

## Purpose

This backlog translates the architectural and runtime concerns identified in the MongoDB assessment into issue-sized implementation work for `Genocs.Persistence.MongoDB`.

The backlog is ordered by runtime risk and delivery impact, not by namespace.

## Current Status

Observed baseline (April 2026):

- MongoDB package builds for net8.0, net9.0, and net10.0, but emits recurring warning noise.
- Repository layer still contains runtime contract gaps (`NotImplementedException`) in exposed code paths.
- Repository implementations contain sync-over-async and fire-and-forget write behavior in critical methods.
- Registration/provider composition may create split `MongoClient` instances across code paths.
- Seeding guard is process-wide and not database-aware.
- Unit test project currently discovers no executable tests.
- Component tests are all skipped and currently SQL Server-oriented, not MongoDB integration-oriented.

Latest validation runs:

- `dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj -c Debug --nologo`

## Planning Assumptions

- Prioritize runtime correctness and contract reliability before feature expansion.
- Remove execution-path exceptions and blocking behavior first.
- Align DI object graph to one authoritative `MongoClient` pipeline.
- Reduce nullable warning noise to improve regression signal quality.
- Restore executable unit and integration coverage before hardening quality gates.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Runtime correctness and repository safety baseline | MONGO-001 to MONGO-003 |
| M2 | DI composition and lifecycle consistency | MONGO-004 to MONGO-005 |
| M3 | Serialization/encryption posture and compatibility | MONGO-006 to MONGO-007 |
| M4 | Testability, docs, and quality gates | MONGO-008 to MONGO-010 |

## Execution Order

1. Complete M1 before any API-surface expansion.
2. Complete M2 before production adoption recommendations.
3. Complete M3 before declaring compatibility and security posture stable.
4. Complete M4 to lock in maintainability and release confidence.

---

## M1: Runtime Correctness and Repository Safety Baseline

### MONGO-001 Remove runtime `NotImplementedException` paths from repository APIs

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

Repository interfaces expose methods that currently throw `NotImplementedException` in implementation paths, causing runtime failures for valid operations.

**Likely touch points**

- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs`

**Acceptance criteria**

- No repository method in public execution paths throws `NotImplementedException`.
- Unsupported behavior is explicitly rejected with clear contract exceptions or removed from surface area.
- Tests cover each previously unimplemented method path.

**Dependencies**

- none

**Implementation notes**

- Implemented `Update(TKey id, Action<TEntity> updateAction)` and `UpdateAsync(TKey id, Func<TEntity, Task> updateAction, CancellationToken cancellationToken)` in `MongoBaseRepository` using get-mutate-persist semantics.
- Implemented `GetByIdAsync(TKey id, CancellationToken cancellationToken)` in `MongoBaseRepositoryOfType` to return `SingleOrDefaultAsync` from Mongo collection queries.
- Added focused tests in `src/tests/Genocs.Persistence.MongoDB.UnitTests/Repository/MongoRepositoryContractTests.cs` covering:
- update-by-id sync path,
- update-by-id async path,
- `GetByIdAsync` path in `MongoBaseRepositoryOfType`.
- Added internals test visibility in `src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj` for `Genocs.Persistence.MongoDB.UnitTests`.
- Validation: `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo` (3 passed, 0 failed).

### MONGO-002 Eliminate sync-over-async and fire-and-forget write operations

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

Synchronous repository methods currently use async driver APIs without awaiting or block using `.Result`, risking deadlocks and non-deterministic persistence behavior.

**Likely touch points**

- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs`

**Acceptance criteria**

- No `.Result`/`.Wait()` remains in repository implementation.
- No fire-and-forget write operation remains in repository implementation.
- Sync methods use sync driver calls, async methods use awaited async driver calls.
- Component tests validate deterministic write semantics.

**Dependencies**

- `MONGO-001`

**Implementation notes**

- Replaced fire-and-forget sync update path in `MongoBaseRepositoryOfType.Update(TEntity entity)` from `ReplaceOneAsync(...)` to synchronous `ReplaceOne(...)`.
- Replaced blocking delete path in `MongoBaseRepositoryOfType.Delete(TKey id)` from `DeleteOneAsync(...).Result` to synchronous `DeleteOne(...)`.
- Updated `MongoBaseRepositoryOfType.AddAsync(...)` to propagate the provided `CancellationToken` to `InsertOneAsync(...)`.
- Added focused unit coverage in `src/tests/Genocs.Persistence.MongoDB.UnitTests/Repository/MongoRepositoryContractTests.cs`:
- `Update_ForRepositoryOfType_UsesSynchronousReplaceOne`
- `Delete_ForRepositoryOfType_UsesSynchronousDeleteOne`
- Validation: `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo` (5 passed, 0 failed).

### MONGO-003 Align repository contract behavior and nullability semantics

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

Nullability and contract signatures drift from base interfaces, generating warning noise and ambiguity for consumers.

**Likely touch points**

- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoBaseRepository.cs`

**Acceptance criteria**

- Repository signatures match base interface nullability intent.
- Hidden-member warnings are resolved (`override/new` semantics explicitly declared as intended).
- Package warning count is materially reduced for repository files.

**Dependencies**

- `MONGO-001`

**Implementation notes**

- Aligned repository return signatures in `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs` with `IRepositoryOfEntity<TEntity, TKey>` contract nullability:
- `FirstOrDefault(TKey id)` now returns `TEntity` (contract-compatible),
- `FirstOrDefaultAsync(...)` overloads now return `Task<TEntity>`,
- `Load(TKey id)` now returns `TEntity`,
- `GetByIdAsync(TKey id, ...)` now returns `Task<TEntity>`.
- Resolved hidden-member warning in `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs` by explicitly overriding `GetByIdAsync(TKey id, CancellationToken cancellationToken = default)` from `RepositoryBase<TEntity, TKey>`.
- Removed the explicit-interface optional-argument warning by updating `IRepositoryOfEntity<TEntity, TKey>.UpdateAsync(...)` explicit implementation signature in `MongoBaseRepository`.
- Replaced several equality predicates with `EqualityComparer<TKey>.Default.Equals(...)` to avoid nullable-id dereference warnings in repository predicate expressions.
- Validation:
- `dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo` (repository warning count reduced from 26 to 4 warnings per target framework; remaining warnings are outside MONGO-003 contract/nullability scope).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo` (5 passed, 0 failed).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj -c Debug --nologo` (0 failed, 3 skipped due to Docker availability conditions).

---

## M2: DI Composition and Lifecycle Consistency

### MONGO-004 Unify `MongoClient`/`IMongoDatabase` composition across provider and extensions

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

Current registration path and provider implementation can instantiate separate `MongoClient` objects, which can produce inconsistent runtime behavior.

**Likely touch points**

- `src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs`
- `src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs`
- `src/Genocs.Persistence.MongoDB/Factories/MongoSessionFactory.cs`

**Acceptance criteria**

- A single authoritative `MongoClient` pipeline is used for repositories, session factory, and provider.
- `MongoDatabaseProvider` consumes DI-registered `IMongoClient` and options instead of creating a new client internally.
- Tracing/session behavior is consistent across all access paths.

**Dependencies**

- `MONGO-002`

**Implementation notes**

- Refactored `src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs` to consume DI-registered dependencies directly:
- constructor now uses `IMongoClient` and `MongoOptions`,
- removed internal `MongoClient` creation so provider no longer builds a separate client pipeline,
- `Database` is resolved from the same injected `IMongoClient` using configured database name.
- Updated `src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs` registration to provide `IMongoDatabase` as an explicit singleton resolved from the same singleton `IMongoClient` and `MongoOptions`.
- This aligns repositories (`IMongoDatabase`), session factory (`IMongoClient`), and provider (`IMongoClient` + `IMongoDatabase`) to one authoritative client/database composition path.
- Added focused unit tests in `src/tests/Genocs.Persistence.MongoDB.UnitTests/MongoDatabaseProviderTests.cs` validating:
- provider reuses injected client and resolves configured database,
- provider rejects invalid options.
- Validation:
- `dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo` (succeeds for net8/net9/net10; warning profile unchanged outside current scope).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo` (7 passed, 0 failed).

### MONGO-005 Make initializer/seed execution database-aware and observable

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

Current static seeding guard blocks initialization process-wide, not per target database, which can skip expected seed behavior.

**Likely touch points**

- `src/Genocs.Persistence.MongoDB/Initializers/MongoDbInitializer.cs`
- `src/Genocs.Persistence.MongoDB/Seeders/MongoSeeder.cs`

**Acceptance criteria**

- Seed idempotency is tracked per database key (and optionally connection+database).
- Initialization decisions are observable through structured diagnostics/logging.
- Integration tests validate seeding semantics across multiple database names in one process.

**Dependencies**

- `MONGO-004`

**Implementation notes**

- Refactored `src/Genocs.Persistence.MongoDB/Initializers/MongoDbInitializer.cs` to replace the process-wide static flag with a database-aware guard:
- added per-database initialization tracking using a static `ConcurrentDictionary<string, byte>`,
- seed key now uses connection string + database name to isolate initialization across different database targets in the same process.
- Added structured diagnostics via `ILogger<MongoInitializer>`:
- logs when seeding is disabled,
- logs when seeding is skipped because the database key was already initialized,
- logs seeding start/completion,
- logs seeding failure and guard reset for retry.
- Added retry-safe behavior: when seeding fails, the database key is removed from the guard map so subsequent initialization attempts can retry deterministically.
- Added focused unit coverage in `src/tests/Genocs.Persistence.MongoDB.UnitTests/Initializers/MongoInitializerTests.cs` validating:
- seed disabled path does not invoke seeder,
- same database is seeded only once per process,
- different database names are seeded independently within one process,
- failed seed attempt clears guard and allows retry.
- Validation:
- `dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo` (succeeds for net8/net9/net10; warning profile reduced to known non-MONGO-005 warnings).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo` (11 passed, 0 failed).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj -c Debug --nologo` (0 failed, 3 skipped due to Docker availability conditions).

---

## M3: Serialization/Encryption Posture and Compatibility

### MONGO-006 Modernize GUID serializer strategy with compatibility controls

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

Global registration currently uses legacy GUID representation, which risks interoperability issues with modern MongoDB driver expectations.

**Likely touch points**

- `src/Genocs.Persistence.MongoDB/Extensions/ServiceCollectionExtensions.cs`
- `src/Genocs.Persistence.MongoDB/Configurations/MongoOptions.cs`

**Acceptance criteria**

- Serializer strategy supports `GuidRepresentation.Standard` as default for new deployments.
- Compatibility mode exists for legacy datasets where migration is not immediate.
- Migration guidance is documented for package consumers.

**Dependencies**

- `MONGO-004`

**Implementation notes**

- Added explicit GUID representation compatibility control in `src/Genocs.Persistence.MongoDB/Configurations/MongoOptions.cs`:
- new `GuidRepresentationMode` setting with `Standard` default,
- compatibility mode option for legacy datasets (`CSharpLegacy`).
- Introduced `src/Genocs.Persistence.MongoDB/Configurations/MongoGuidRepresentationMode.cs` enum to define supported serializer modes.
- Updated `src/Genocs.Persistence.MongoDB/Extensions/ServiceCollectionExtensions.cs`:
- convention registration now accepts configured representation mode,
- GUID serializer registration now maps to `GuidRepresentation.Standard` by default and `GuidRepresentation.CSharpLegacy` when compatibility mode is selected.
- Updated `src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs` to pass `MongoOptions.GuidRepresentationMode` into convention registration.
- Added focused unit coverage in `src/tests/Genocs.Persistence.MongoDB.UnitTests/GuidRepresentationCompatibilityTests.cs` validating:
- default options mode is `Standard`,
- configured mode maps to expected MongoDB `GuidRepresentation` values (`Standard` / `CSharpLegacy`).
- Published migration guidance for consumers:
- `src/Genocs.Persistence.MongoDB/README_NUGET.md` now documents default behavior, compatibility mode, and a step-by-step legacy migration path.
- `src/Genocs.Persistence.MongoDB/_docs/README_NUGET.md` aligned with the same guidance.
- Validation:
- `dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo` (succeeds for net8/net9/net10).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo` (14 passed, 0 failed).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj -c Debug --nologo` (0 failed, 3 skipped due to Docker availability conditions).

### MONGO-007 Resolve encryption roadmap ambiguity (complete or deprecate)

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

Encryption options and initializer surface exist but are not fully wired in runtime composition, creating capability ambiguity.

**Likely touch points**

- `src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs`
- `src/Genocs.Persistence.MongoDB/Encryptions/AzureInitializer.cs`
- `src/Genocs.Persistence.MongoDB/Configurations/MongoEncryptionOptions.cs`

**Acceptance criteria**

- One clear decision is implemented:
- encryption path is fully wired, validated, and documented, or
- unfinished encryption APIs are marked obsolete/deprecated with explicit guidance.
- Tests verify selected behavior path.

**Dependencies**

- `MONGO-004`

**Implementation notes**

- Chosen path: remove unfinished encryption API surface from package runtime composition.
- Deleted obsolete encryption artifacts:
- `src/Genocs.Persistence.MongoDB/Configurations/MongoEncryptionOptions.cs`
- `src/Genocs.Persistence.MongoDB/Encryptions/AzureInitializer.cs`
- Updated agent documentation to remove stale references to `mongoDbEncryption`/`MongoEncryptionOptions` and clarify that built-in client-side field encryption is not provided by this package.
- Validation:
- `dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo` (succeeds for net8/net9/net10).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo` (all tests pass).
- `dotnet test src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj -c Debug --nologo` (0 failed; Docker-gated tests skipped where Docker is unavailable).

---

## M4: Testability, Documentation, and Quality Gates

### MONGO-008 Establish executable unit tests for repository/registration contracts

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

Current unit test project does not execute meaningful tests, leaving critical repository and registration behavior unprotected.

**Likely touch points**

- `src/tests/Genocs.Persistence.MongoDB.UnitTests`
- `src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/*`

**Acceptance criteria**

- Unit test project includes executable tests for:
- repository CRUD and query semantics,
- registration/option validation behavior,
- initializer guard behavior.
- Unit test run reports non-zero executed test count.

**Dependencies**

- `MONGO-001` to `MONGO-005`

**Implementation notes**

- Expanded repository contract unit coverage in `src/tests/Genocs.Persistence.MongoDB.UnitTests/Repository/MongoRepositoryContractTests.cs` to include additional CRUD/query assertions:
- `AddAsync_ForRepositoryOfType_UsesInsertOneAsyncWithCancellationToken`,
- `DeleteAsync_ById_UsesDeleteOneAsync`,
- `FirstOrDefaultAsync_ById_ReturnsEntityWhenFound`.
- Added dedicated registration/options validation tests in `src/tests/Genocs.Persistence.MongoDB.UnitTests/MongoRegistrationExtensionsTests.cs` covering:
- core Mongo service registration for valid options (`IMongoClient`, `IMongoDatabase`, `IMongoInitializer`, `IMongoSessionFactory`, `IMongoDatabaseProvider`, `IMongoSeeder`),
- invalid options short-circuit behavior (no core Mongo registrations),
- `AddMongoWithRegistration` open generic repository registration,
- typed `AddMongoRepository<TEntity, TKey>` binding.
- Existing initializer guard behavior remains covered by `src/tests/Genocs.Persistence.MongoDB.UnitTests/Initializers/MongoInitializerTests.cs`.
- Validation:
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo` (non-zero executed tests, all passing).

### MONGO-009 Replace SQL-based component test scaffold with MongoDB integration tests

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

Component tests are currently SQL Server-centric and skipped, providing no MongoDB runtime assurance.

**Likely touch points**

- `src/tests/Genocs.Persistence.MongoDB.ComponentTests/DriverRepositoryTests.cs`
- `src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj`

**Acceptance criteria**

- Component tests use MongoDB Testcontainers and exercise real Mongo repository flows.
- At least one integration test runs successfully in CI-enabled environments.
- Skip conditions are explicit, minimal, and environment-driven (not permanent placeholders).

**Dependencies**

- `MONGO-002`
- `MONGO-004`

**Implementation notes**

- Replaced SQL Server Testcontainers scaffold with MongoDB Testcontainers in `src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj`:
- package reference moved from `Testcontainers.MsSql` to `Testcontainers.MongoDb`,
- added project reference to `src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj` so component tests exercise the package runtime implementation directly.
- Replaced placeholder test file with MongoDB integration tests in `src/tests/Genocs.Persistence.MongoDB.ComponentTests/DriverRepositoryTests.cs`:
- added class-level container lifecycle using `IAsyncLifetime`,
- introduced real repository flow tests using `MongoBaseRepositoryOfType<DriverEntity, Guid>` against a live MongoDB container,
- covered add/get (`AddAsync_ThenGetByIdAsync_PersistsDocumentInMongoDb`) and update/delete (`UpdateAndDelete_PersistsMutationAndRemovalInMongoDb`) behavior.
- Added explicit environment-driven skip gating using xUnit v3 `SkipUnless` with Docker availability probing (`docker info`) to avoid permanent hardcoded skips while still allowing CI-enabled environments with Docker to execute integration tests.
- Validation:
- `dotnet test src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj -c Debug --nologo` (project builds; tests run when Docker is available, otherwise are skipped by explicit environment condition).

### MONGO-010 Publish migration and usage guidance for stabilized behavior

**Status**: Completed (April 2026)

**Priority**: P2

**Problem**

Consumer-facing guidance must reflect finalized DI, repository, serializer, and encryption decisions to avoid adoption drift.

**Likely touch points**

- `docs/Genocs.Persistence.MongoDB-Agent-Documentation.md`
- `src/Genocs.Persistence.MongoDB/README_NUGET.md`
- `CHANGELOG.md`

**Acceptance criteria**

- Documentation aligns with implemented behavior for registration paths and repository contracts.
- Breaking or behavior-changing updates are recorded in changelog.
- Guidance includes migration notes for serializer and any API deprecations.

**Dependencies**

- `MONGO-006`
- `MONGO-007`
- `MONGO-008`
- `MONGO-009`

**Implementation notes**

- Updated `docs/Genocs.Persistence.MongoDB-Agent-Documentation.md` to align with effective package behavior and improve AI-agent guidance quality:
- synchronized runtime statements for seeding guard semantics (per database key) and GUID serializer defaults,
- expanded completed-task status through `MONGO-009`,
- added explicit agent execution protocol and corrected decision checklist semantics.
- Updated consumer-facing package guidance:
- `src/Genocs.Persistence.MongoDB/README_NUGET.md` now documents stabilized runtime behavior (single DI-managed client composition, seeding retry semantics) and explicit encryption-surface guidance,
- `src/Genocs.Persistence.MongoDB/_docs/README_NUGET.md` aligned with the same behavior/migration guidance.
- Recorded behavior and migration documentation updates in `CHANGELOG.md` under `Unreleased`.
- Validation:
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj -c Debug --nologo`

## Definition of Done (Backlog Level)

- P0 tasks are completed and validated in CI.
- Package no longer contains runtime `NotImplementedException` paths in public operational flows.
- MongoDB repository/test baseline is executable and non-placeholder.
- Consumer documentation reflects current supported behavior and migration guidance.
