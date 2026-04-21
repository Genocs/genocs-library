# Genocs.Persistence.MongoDB Assessment (April 2026)

## Purpose

This assessment evaluates the current `Genocs.Persistence.MongoDB` implementation to identify runtime risks, architectural concerns, and quality gaps, then proposes prioritized improvements aligned with the existing Genocs backlog style.

## Assessment Scope

Reviewed package and tests:

- `src/Genocs.Persistence.MongoDB`
- `src/tests/Genocs.Persistence.MongoDB.UnitTests`
- `src/tests/Genocs.Persistence.MongoDB.ComponentTests`

Key files inspected include:

- `src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs`
- `src/Genocs.Persistence.MongoDB/Extensions/ServiceCollectionExtensions.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs`
- `src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs`
- `src/Genocs.Persistence.MongoDB/Initializers/MongoDbInitializer.cs`
- `src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs`
- `src/tests/Genocs.Persistence.MongoDB.UnitTests/EncryptionUnitTest.cs`
- `src/tests/Genocs.Persistence.MongoDB.ComponentTests/DriverRepositoryTests.cs`

## Validation Evidence

Executed on 2026-04-20:

- `dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Persistence.MongoDB.UnitTests/Genocs.Persistence.MongoDB.UnitTests.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Persistence.MongoDB.ComponentTests/Genocs.Persistence.MongoDB.ComponentTests.csproj -c Debug --nologo`

Observed baseline:

- Package build succeeds for net8.0/net9.0/net10.0, but with repeated warnings in MongoDB package (25 warnings per target framework).
- Unit test project runs with `0` discovered tests.
- Component test project reports `3` tests, all skipped.

## Executive Summary

The package compiles and exposes the expected MongoDB integration surface, but it is not production-hardened yet. Several repository paths still contain runtime `NotImplementedException` behavior, sync-over-async calls, and contract/nullability drift. Registration and provider lifetimes also diverge in ways that can produce inconsistent Mongo client/database behavior.

The most urgent work is to remove runtime contract gaps and fix repository correctness behavior. Immediately after that, align DI composition and nullability contracts, then restore meaningful test coverage.

## Findings (Ordered by Severity)

### 1) Runtime contract gaps in repository APIs

Severity: Critical

Where:

- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs`

Evidence:

- `MongoBaseRepository.Update(TKey id, Action<TEntity> updateAction)` throws `NotImplementedException`.
- `MongoBaseRepository.UpdateAsync(TKey id, Func<TEntity, Task> updateAction, CancellationToken cancellationToken)` throws `NotImplementedException`.
- `MongoBaseRepositoryOfType.GetByIdAsync(...)` throws `NotImplementedException`.

Risk:

- Runtime failures in valid repository call paths.
- Contract appears available at compile time but breaks at runtime.

Recommendation:

- Implement or explicitly remove/disable unsupported methods with clear contract exceptions.
- Add tests for all CRUD methods exposed by interfaces.

### 2) Sync-over-async and fire-and-forget behavior in write paths

Severity: High

Where:

- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs`

Evidence:

- `Update(TEntity entity)` calls `ReplaceOneAsync(...)` without awaiting completion.
- `Delete(TKey id)` calls `DeleteOneAsync(...).Result`.

Risk:

- Potential deadlocks and thread-pool blocking.
- Write operations can appear completed before persistence actually finishes.

Recommendation:

- Use synchronous driver methods in synchronous APIs (`ReplaceOne`, `DeleteOne`) or convert APIs to async-only in vNext.
- Eliminate `.Result`/`.Wait()` in package code.

### 3) DI composition inconsistency can create split Mongo client/database state

Severity: High

Where:

- `src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs`
- `src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs`

Evidence:

- `AddMongo(...)` registers `IMongoClient` singleton from options.
- `AddMongo(...)` registers `IMongoDatabase` as transient from that singleton client.
- `MongoDatabaseProvider` constructs a separate `MongoClient` instance from options instead of consuming DI `IMongoClient`.

Risk:

- `IMongoSessionFactory` and repository/provider paths may operate on different client instances.
- Harder diagnostics and inconsistent behavior under advanced client configuration.

Recommendation:

- Refactor `MongoDatabaseProvider` to depend on DI-registered `IMongoClient` and `MongoOptions`, then resolve `IMongoDatabase` from that client only.
- Keep one authoritative client pipeline for tracing/session settings.

### 4) Seeding guard is process-wide, not database-aware

Severity: High

Where:

- `src/Genocs.Persistence.MongoDB/Initializers/MongoDbInitializer.cs`

Evidence:

- Static `_initialized` gate blocks subsequent seeding after first initialization in process lifetime.

Risk:

- Multi-tenant/integration scenarios that vary database names can silently skip seeding.
- Non-deterministic startup behavior when multiple hosts initialize within same process.

Recommendation:

- Track initialization per database name (or per connection+database key), not globally.
- Add explicit logging/diagnostic event when seeding is skipped due to guard state.

### 5) BSON Guid representation uses legacy mode

Severity: Medium

Where:

- `src/Genocs.Persistence.MongoDB/Extensions/ServiceCollectionExtensions.cs`

Evidence:

- Registers `GuidSerializer(GuidRepresentation.CSharpLegacy)` globally.

Risk:

- Legacy representation can break interoperability with newer drivers/services using standard representation.
- Global serializer registration affects the entire process.

Recommendation:

- Move to `GuidRepresentation.Standard` strategy for vNext with migration guidance.
- Add compatibility mode toggle if existing persisted data depends on legacy format.

### 6) Nullability and contract drift remain unresolved

Severity: Medium

Where:

- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs`
- `src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs`
- `src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs`

Evidence:

- Build warnings include CS0114, CS8613, CS8766, CS8602, CS1066 and pagination nullability warnings.

Risk:

- Warning noise obscures real regressions.
- Interface contract ambiguity (`TEntity` vs `TEntity?`) increases misuse risk.

Recommendation:

- Align repository signatures to base interfaces and nullability intent.
- Remove hidden-member behavior by using explicit `override` where required.
- Harden paging API defaults and null guards.

### 7) Registration path uses silent no-op and console output for failure handling

Severity: Medium

Where:

- `src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs`

Evidence:

- Missing config section returns builder silently.
- Invalid options and duplicate registration paths emit `Console.WriteLine(...)` and continue.

Risk:

- Misconfiguration can pass unnoticed until runtime behavior fails later.

Recommendation:

- Use typed logging and deterministic failure strategy (throw for invalid options in strict mode).
- At minimum, emit structured diagnostics through Genocs startup diagnostics.

### 8) Encryption surface is partially wired and currently inert

Severity: Medium

Where:

- `src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs`
- `src/Genocs.Persistence.MongoDB/Encryptions/AzureInitializer.cs`
- `src/Genocs.Persistence.MongoDB/Configurations/MongoEncryptionOptions.cs`

Evidence:

- Encryption initialization in provider is commented out.
- `AzureInitializer` contains a fully commented implementation block.
- Encryption options are injected but not actively used.

Risk:

- API implies encryption capability that is effectively unavailable.
- Consumers may assume data-at-rest protections are active.

Recommendation:

- Either remove/deprecate unfinished encryption API surface or complete and test end-to-end.
- Document current status explicitly in package docs.

### 9) Test posture does not protect package behavior

Severity: High

Where:

- `src/tests/Genocs.Persistence.MongoDB.UnitTests/EncryptionUnitTest.cs`
- `src/tests/Genocs.Persistence.MongoDB.ComponentTests/DriverRepositoryTests.cs`

Evidence:

- Unit tests discover no executable tests.
- Component tests are SQL Server based, all skipped, and not Mongo-focused.

Risk:

- Critical repository and registration regressions can ship undetected.

Recommendation:

- Add focused unit tests for repository methods, registration behavior, option validation, and seeding guard logic.
- Replace SQL Server component test scaffold with MongoDB Testcontainers integration tests.
- Ensure at least one non-skipped integration test runs in CI.

## Suggested Improvement Backlog (Proposed)

1. `MONGO-001` Remove all repository `NotImplementedException` paths and align contracts.
2. `MONGO-002` Eliminate sync-over-async and blocking calls in repository implementations.
3. `MONGO-003` Unify Mongo client/database DI wiring across provider/session/repository paths.
4. `MONGO-004` Make seeding idempotency database-aware, not process-global.
5. `MONGO-005` Resolve nullability signature drift and warning hotspots.
6. `MONGO-006` Migrate GUID serialization strategy with compatibility plan.
7. `MONGO-007` Replace silent registration failures with deterministic diagnostics/fail-fast policy.
8. `MONGO-008` Decide encryption roadmap: complete implementation or deprecate surface.
9. `MONGO-009` Rebuild test strategy with executable unit + Mongo integration tests.

## Conclusion

`Genocs.Persistence.MongoDB` has the right package boundaries and extension points, but runtime safety and quality controls are currently below the bar for a stable persistence package. Prioritizing `MONGO-001` through `MONGO-004` will remove the most serious production risks. Completing `MONGO-005` through `MONGO-009` will improve maintainability, confidence, and consumer adoption readiness.
