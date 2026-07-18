# Genocs.Persistence.EFCore Implementation Backlog

## Purpose

This backlog translates the architectural and runtime concerns identified in the EFCore assessment into issue-sized implementation work for `Genocs.Persistence.EFCore`.

The backlog is ordered by runtime risk and delivery impact, not by namespace.

## Current Status

Observed baseline (May 2026):

- Package builds for `net8.0`, `net9.0`, and `net10.0` with pre-existing warning noise.
- `InitializeApplicationDbForTenantAsync` previously threw `NotImplementedException`; now implemented via Finbuckle scope injection.
- `ApplicationDbInitializer` previously never applied migrations; now gates on `DatabaseOptions.AutoApplyMigrations` and calls `MigrateAsync` or stops the host.
- Repository auto-registration scans the wrong assembly — no consumer repositories are ever registered.
- Multi-tenancy extension method contains unreachable code — `ITenantService` and EF Core tenant store are never registered.
- MongoDB `UseDatabase` path uses a hardcoded `"DatabaseName"` literal.
- No unit or integration test project exists.

Package split (July 2026):

`Genocs.Persistence.EFCore` was split into a provider-agnostic core plus dedicated packages. Provider-specific logic (DbContext configuration, connection string securing/validation, migration support) now lives behind the `IEFCoreDbProvider` abstraction, with one implementation per provider package:

- `Genocs.Persistence.EFCore` — core: repositories, initialization pipeline, `DatabaseOptions`, provider abstraction. No database driver references.
- `Genocs.Persistence.EFCore.SqlServer` / `.PostgreSQL` / `.MySql` / `.Sqlite` / `.Oracle` / `.MongoDB` — one `IEFCoreDbProvider` implementation each, registered via `Add[Provider]DbProvider()`.
- `Genocs.Persistence.EFCore.MultiTenancy.SqlServer` — the whole Finbuckle multitenancy surface (`GNXTenantInfo`, `TenantDbContext`, `TenantService`, tenant requests), with the tenant store pinned to SQL Server. Tenant-scoped initialization moved to `ITenantDatabaseInitializer` so the core `IDatabaseInitializer` contract stays tenant-free.

File paths in older backlog items below refer to the pre-split layout; multitenancy files now live in `src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/` and provider files in their respective `src/Genocs.Persistence.EFCore.[Provider]/` projects.

Latest validation runs:

- `dotnet build src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/Genocs.Persistence.EFCore.MultiTenancy.SqlServer.csproj -v q --nologo` (builds the full package family for net8/net9/net10)
- `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -v q --nologo` (33/33 passing)

## Planning Assumptions

- Backward compatibility is not a requirement. Breaking changes are acceptable to reach correctness.
- Prioritize runtime correctness and registration safety before feature expansion.
- Remove `NotImplementedException` paths and broken registration logic first.
- Dependency hygiene (removing `MongoDB.Driver`, `Serilog.Sinks.MSSqlServer`) follows correctness work.
- Test coverage is required before any milestone is considered complete.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Runtime correctness baseline | EFCORE-001 to EFCORE-005 |
| M2 | DI, registration, and dependency hygiene | EFCORE-006 to EFCORE-015 |
| M3 | Testability and quality gates | EFCORE-016 to EFCORE-019 |
| M4 | Polishing, documentation, and release readiness | EFCORE-020 to EFCORE-025 |

## Execution Order

1. Complete M1 before any API-surface expansion or multi-tenancy feature work.
2. Complete M2 before production adoption recommendations.
3. Complete M3 before declaring the package stable for library release.
4. Complete M4 to lock in maintainability and consumer-facing documentation.

---

## M1: Runtime Correctness Baseline

### EFCORE-001 Implement `InitializeApplicationDbForTenantAsync` using Finbuckle scope injection

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

`DatabaseInitializer.InitializeApplicationDbForTenantAsync` threw `NotImplementedException`. This method is called from `TenantService.CreateAsync`, so every tenant creation call crashed at runtime.

**Touch points**

- `src/Genocs.Persistence.EFCore/Initialization/DatabaseInitializer.cs`

**Acceptance criteria**

- Method no longer throws `NotImplementedException`.
- Tenant initialization runs in an isolated DI scope.
- The Finbuckle tenant context is set in the scope so downstream scoped services can read tenant details.
- Tenants with a dedicated connection string redirect their scoped `ApplicationDbContext` before initialization runs.

**Dependencies**

- none

**Implementation notes**

- Replaced `throw new NotImplementedException()` with a full implementation in `DatabaseInitializer.InitializeApplicationDbForTenantAsync`.
- Creates a child `IServiceScope` per tenant to isolate initialization from the ambient HTTP request context.
- Resolves `IMultiTenantContextSetter` from the scope and assigns a new `MultiTenantContext<GNXTenantInfo>` containing the provided tenant — this makes the tenant identity visible to all scoped services resolved within the initialization scope (including `ApplicationDbSeeder` once it is implemented).
- If `tenant.ConnectionString` is non-empty, calls `dbContext.Database.SetConnectionString(tenant.ConnectionString)` on the scoped `ApplicationDbContext` before delegating to `ApplicationDbInitializer.InitializeAsync`, redirecting the scoped context to the tenant-specific database.
- Tenants sharing the root connection string (empty `ConnectionString`) fall through unchanged.
- Validation: `dotnet build src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj -c Debug --nologo` (succeeds for net8/net9/net10; warning profile unchanged).

---

### EFCORE-002 Apply pending migrations on startup with `AutoApplyMigrations` safety gate

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

`ApplicationDbInitializer.InitializeAsync` detected migrations via `GetMigrations()` but never applied them — the method ended with `await Task.CompletedTask`. Consumers relying on auto-migration at startup received no schema.

**Touch points**

- `src/Genocs.Persistence.EFCore/Configurations/DatabaseOptions.cs`
- `src/Genocs.Persistence.EFCore/Initialization/ApplicationDbInitializer.cs`

**Acceptance criteria**

- A new `AutoApplyMigrations` configuration flag (default `false`) controls migration behavior.
- When `true`: pending migrations are logged by name and applied via `MigrateAsync`.
- When `false`: pending migrations are logged as an error with names listed, and the host is stopped gracefully via `IHostApplicationLifetime.StopApplication()`.
- No migration behavior occurs when no pending migrations exist.
- MongoDB provider path is skipped (unchanged).

**Dependencies**

- none

**Implementation notes**

- Added `public bool AutoApplyMigrations { get; set; } = false` to `DatabaseOptions` with an XML doc comment explaining the safety intent.
- Rewrote `ApplicationDbInitializer.InitializeAsync`:
  - Changed from `GetMigrations()` (all registered) to `GetPendingMigrationsAsync()` (only unapplied).
  - No pending migrations → log info, return.
  - Pending + `AutoApplyMigrations = false` → `LogError` with pending migration names, call `_applicationLifetime.StopApplication()`, return.
  - Pending + `AutoApplyMigrations = true` → log migration names, call `await _dbContext.Database.MigrateAsync(cancellationToken)`.
- `ApplicationDbInitializer` constructor now injects `IHostApplicationLifetime` and `IOptions<DatabaseOptions>` in addition to existing parameters.
- Validation: `dotnet build src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj -c Debug --nologo` (succeeds for net8/net9/net10; warning profile unchanged).

---

### EFCORE-003 Fix `AddRepositories` to scan the consuming application's assembly

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

`EFCoreExtensions.AddRepositories` iterates `typeof(IAggregateRoot).Assembly`, which always resolves to the `Genocs.Common` assembly. No consumer-defined aggregate root types are ever found, so `IRepository<T>`, `IReadRepository<T>`, and `IRepositoryWithEvents<T>` are never registered. Any attempt to resolve a repository in a consumer application causes `InvalidOperationException` at runtime.

**Touch points**

- `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`

**Acceptance criteria**

- Repository auto-registration discovers aggregate root types from the consuming application's entry assembly (or a caller-supplied assembly).
- `IRepository<T>`, `IReadRepository<T>`, and `IRepositoryWithEvents<T>` are registered for each discovered aggregate root type.
- `AddEFCorePersistence` accepts an optional `Assembly[]` parameter to allow callers to explicitly provide additional assemblies for scanning.
- Fallback behavior (no assemblies supplied) scans `Assembly.GetEntryAssembly()`.

**Dependencies**

- none

**Implementation notes**

- Updated `AddEFCorePersistence` to accept caller-supplied additional assemblies (optional `params Assembly[]`).
- Updated `AddRepositories` assembly resolution to always include `Assembly.GetEntryAssembly()` and merge it with caller-supplied assemblies, deduplicated.
- This prevents scanning from being limited to `Genocs.Common` and enables discovery of consumer aggregate roots in entry and feature assemblies.
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -c Debug --nologo` (12/12 passing).

---

### EFCORE-004 Fix unreachable code in `AddFinbuckleMultiTenancy` to enable EF Core tenant store registration

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

The `IServiceCollection.AddFinbuckleMultiTenancy<TTenantInfo>` overload in `Multitenancy/Extensions.cs` contains an early `return services;` on line 68. Everything after it — including `AddDbContext<TenantDbContext>`, the EF Core store wiring, and `AddScoped<ITenantService, TenantService>()` — is unreachable dead code (compiler warning `CS0162` on all targets). `ITenantService` is never registered.

**Touch points**

- `src/Genocs.Persistence.EFCore/Multitenancy/Extensions.cs`

**Acceptance criteria**

- The unreachable code block is removed.
- The method is restructured into two clearly-separated overloads: one using `ConfigurationStore` (current first path), one using `EFCoreStore` (current dead path), or the dead block is promoted to a new dedicated `AddFinbuckleMultiTenancyWithEfCoreStore` overload that was already scaffolded lower in the file.
- `ITenantService` is reachable and registered when the EF Core store path is used.
- Compiler warning `CS0162` is resolved.

**Dependencies**

- none

**Implementation notes**

- Split registration paths in `Multitenancy/Extensions.cs` so `AddFinbuckleMultiTenancy<TTenantInfo>` is configuration-store only.
- Added dedicated EF Core store overloads:
  - `AddFinbuckleMultiTenancyWithEfCoreStore(this IServiceCollection services)`
  - `AddFinbuckleMultiTenancyWithEfCoreStore(this IGenocsBuilder builder)`
- Moved `TenantDbContext` + `WithEFCoreStore<TenantDbContext, GNXTenantInfo>()` + `ITenantService` registration into the dedicated EF Core store path.
- Added registration contract tests in `src/tests/Genocs.Persistence.EFCore.UnitTests/Multitenancy/AddFinbuckleMultiTenancyRegistrationTests.cs` to verify:
  - configuration-store path does not register `ITenantService`
  - EF Core store path registers `ITenantService` and `TenantDbContext`
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -c Debug --nologo` (14/14 passing).

---

### EFCORE-005 Fix hardcoded `"DatabaseName"` literal in MongoDB `UseDatabase` path

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

When `DBProvider` is `mongodb`, `UseDatabase` calls `builder.UseMongoDB(connectionString, "DatabaseName")` with a literal string. Every MongoDB-backed consumer connects to a database named `"DatabaseName"` regardless of configuration.

**Touch points**

- `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`
- `src/Genocs.Persistence.EFCore/Configurations/DatabaseOptions.cs`

**Acceptance criteria**

- MongoDB database name is read from `DatabaseOptions` (new `DatabaseName` property or parsed from the connection string).
- `UseDatabase` passes the configured name to `UseMongoDB`.
- The duplicate `UseDatabase` in `Multitenancy/Extensions.cs` is aligned (see EFCORE-018).

**Dependencies**

- none

**Implementation notes**

- Added `DatabaseName` to `DatabaseOptions` and introduced `GetMongoDatabaseName()` resolution logic:
  - Prefer explicit `DatabaseOptions.DatabaseName`
  - Fallback to parsing MongoDB database name from the connection string path (e.g. `mongodb://host:27017/bookstore`)
- Added MongoDB-specific validation in `DatabaseOptions.Validate(...)` to fail fast when provider is `mongodb` and no database name can be resolved.
- Updated `EFCoreExtensions.UseDatabase(...)` MongoDB path to use resolved database name instead of hardcoded literal.
- Updated `Multitenancy/Extensions.UseDatabase(...)` and its `AddDbContext<TenantDbContext>` call to pass the resolved MongoDB database name as well.
- Added/updated unit tests in `src/tests/Genocs.Persistence.EFCore.UnitTests/Configurations/DatabaseOptionsMongoDbTests.cs` covering:
  - explicit `DatabaseName` precedence,
  - connection-string path parsing,
  - validation failure when no database name is resolvable.
- Added missing `MongoDB` constant to `Multitenancy/DbProviderKeys.cs` so the current duplicate key set compiles with MongoDB support.
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -c Debug --nologo` (17/17 passing).

---

## M2: DI, Registration, and Dependency Hygiene

### EFCORE-006 Replace reflection-based `DomainEventExtensions.AddDomainEvent` with interface contract

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

`DomainEventExtensions.AddDomainEvent` locates the aggregate root's `_domainEvents` backing list via `BindingFlags.NonPublic | BindingFlags.Instance` reflection. Any entity whose backing field is named differently causes a runtime `InvalidOperationException` with no compile-time protection.

**Touch points**

- `src/Genocs.Persistence.EFCore/Repositories/DomainEventExtensions.cs`
- `Genocs.Common` — `IGeneratesDomainEvents` interface (may require extension)

**Acceptance criteria**

- `IGeneratesDomainEvents` exposes an `AddDomainEvent(IEvent)` method, or a parallel `IDomainEventContainer` interface is introduced.
- `DomainEventExtensions.AddDomainEvent` calls the interface method instead of using reflection.
- No `BindingFlags` reflection remains in the domain event path.

**Dependencies**

- none

**Implementation notes**

- Extended `IGeneratesDomainEvents` in `Genocs.Common` with `AddDomainEvent(IEvent @event)` to make event mutation an explicit interface contract.
- Implemented the new contract in `Genocs.Core.Domain.Entities.AggregateRoot<TPrimaryKey>` by appending to the existing `DomainEvents` list with null-guarding.
- Reworked `Genocs.Persistence.EFCore.Repositories.DomainEventExtensions.AddDomainEvent(...)` to delegate directly to `IGeneratesDomainEvents.AddDomainEvent(...)`.
- Removed reflection-based private-field access and all `BindingFlags` usage from the domain-event path.
- Updated EFCore repository test aggregates to implement the new contract method.
- Added targeted tests:
  - `src/tests/Genocs.Persistence.EFCore.UnitTests/Repositories/DomainEventExtensionsTests.cs`
  - Updated `src/tests/Genocs.Core.UnitTests/Domain/Entities/AggregateRootTests.cs` with contract-level add-event verification.
- Validation:
  - `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -c Debug --nologo` (19/19 passing)
  - `dotnet test src/tests/Genocs.Core.UnitTests/Genocs.Core.UnitTests.csproj -c Debug --nologo` (85/85 passing)

---

### EFCORE-007 Remove `MongoDB.Driver` direct dependency from the EFCore package

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

`MongoDB.Driver` is an unconditional `<PackageReference>` in the EFCore `.csproj`. It was added solely so `ConnectionStringSecurer` can parse MongoDB URIs using `MongoUrlBuilder`. This forces every EFCore consumer — including those using only SQL Server — to carry the full MongoDB driver.

**Touch points**

- `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj`
- `src/Genocs.Persistence.EFCore/Configurations/ConnectionStringSecurer.cs`

**Acceptance criteria**

- `MongoDB.Driver` is removed from the unconditional `<ItemGroup>`.
- MongoDB connection string masking is implemented without the driver — use `Uri`/`UriBuilder` to parse `mongodb://` URIs, or guard the `MongoUrlBuilder` usage behind the `Condition="'$(TargetFramework)' == 'netX.X'"` conditional that already imports `MongoDB.EntityFrameworkCore`.
- Build succeeds with no `MongoDB.Driver` direct dependency in the unconditional item group.

**Dependencies**

- EFCORE-005 (ensures MongoDB path is coherently supported)

**Implementation notes**

- Removed unconditional `<PackageReference Include="MongoDB.Driver" ... />` from `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj`.
- Replaced `MongoDB.Driver.MongoUrlBuilder` usage in `ConnectionStringSecurer.MakeSecureMongoDBConnectionString(...)` with driver-free parsing/masking logic:
  - Preferred path uses `Uri`/`UriBuilder` for valid MongoDB URIs.
  - Added fallback string parsing for valid MongoDB multi-host authority formats that `Uri` parsing may not handle.
  - Masks username/password as `*******` while preserving host list, database path, and query/options.
- Added unit tests in `src/tests/Genocs.Persistence.EFCore.UnitTests/Configurations/ConnectionStringSecurerTests.cs` for:
  - single-host MongoDB credential masking,
  - multi-host MongoDB credential masking,
  - unchanged output when no credentials are present.
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -c Debug --nologo` (22/22 passing).

---

### EFCORE-008 Remove or implement `DapperRepository`

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

`DapperRepository.cs` is entirely wrapped in a block comment. `ApplicationDbContext` still exposes `IDbConnection Connection` to support it, but no Dapper package reference exists. The dead code creates confusion about supported features and exposes a raw `IDbConnection` surface with no guarded usage path.

**Touch points**

- `src/Genocs.Persistence.EFCore/Repositories/DapperRepository.cs`
- `src/Genocs.Persistence.EFCore/Context/ApplicationDbContext.cs`

**Acceptance criteria**

- Either: `DapperRepository.cs` is deleted, `IDbConnection Connection` is removed from `ApplicationDbContext`, and all Dapper-related namespace imports are removed.
- Or: A `Dapper` package reference is added, `DapperRepository` is uncommented and made functional, and a corresponding `IDapperRepository` interface is registered in DI.
- No dead commented-out class files remain.

**Dependencies**

- none

**Implementation notes**

- Replaced the commented placeholder in `src/Genocs.Persistence.EFCore/Repositories/DapperRepository.cs` with a concrete `DapperRepository : IDapperRepository` implementation.
- Added `Dapper` package dependency to `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj`.
- Wired `IDapperRepository` in DI via `AddTransient<IDapperRepository, DapperRepository>()` inside `AddEFCorePersistence`.
- Implemented query methods using `CommandDefinition` to flow `CancellationToken`:
  - `QueryAsync<T>`
  - `QueryFirstOrDefaultAsync<T>`
  - `QuerySingleAsync<T>`
- Kept the original tenant-aware SQL placeholder comments as scaffolding for future multitenancy-aware Dapper filtering.
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -c Debug --nologo` (22/22 passing).

---

### EFCORE-009 Replace deprecated `ITransientService`/`IScopedService` with current dependency interfaces

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

`EFCoreExtensions.AddServices` references `ITransientService` and `IScopedService`, both marked `[Obsolete]`. The compiler emits `CS0618` on all target frameworks.

**Touch points**

- `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`

**Acceptance criteria**

- `ITransientService` replaced with `ITransientDependency` from `Genocs.Common.Dependency`.
- `IScopedService` replaced with `IScopedDependency` from `Genocs.Common.Dependency`.
- `CS0618` warnings are resolved.

**Dependencies**

- none

**Implementation notes**

- Verified `Genocs.Persistence.EFCore` no longer references obsolete `ITransientService`/`IScopedService` markers in service-registration paths.
- Removed stale `using Genocs.Common.Interfaces;` from `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs` so the package no longer imports the obsolete marker-interfaces namespace.
- Current registrations use non-obsolete contracts (`IDapperRepository : ITransientDependency` and explicit DI registrations), and no `CS0618` warning for obsolete service markers is emitted by this package.
- Validation: `dotnet build src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj -c Debug --nologo`.

---

### EFCORE-010 Update release build `Genocs.Core` reference to a stable, version-aligned package

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

The release `<ItemGroup>` pins `Genocs.Core` to `9.0.0-beta007` — a pre-release version that predates the package's own `net10.0` target framework support.

**Touch points**

- `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj`

**Acceptance criteria**

- `Genocs.Core` release reference uses a stable (non-pre-release) version number.
- Version is aligned with the published stable release of `Genocs.Core` that supports `net8.0`, `net9.0`, and `net10.0`.

**Dependencies**

- none

**Implementation notes**

- The stale release-only package pin (`Genocs.Core` pre-release) is no longer present.
- `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj` now references `Genocs.Core` via source `ProjectReference` for repository builds, avoiding any hard-pinned pre-release NuGet dependency.
- Confirmed there is no `<PackageReference Include="Genocs.Core" Version="*-beta*" />` in this package.
- Validation: `dotnet build src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj -c Debug --nologo`.

---

### EFCORE-011 Implement or remove `ApplicationDbSeeder` placeholder methods

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

All three methods in `ApplicationDbSeeder` (`SeedRolesAsync`, `AssignPermissionsToRoleAsync`, `SeedAdminUserAsync`) consist entirely of commented-out code followed by `await Task.CompletedTask`. No actual seeding occurs. `AssignPermissionsToRoleAsync` also has an incorrect signature (parameters were stripped when the body was commented out). The class is registered in DI and called from `ApplicationDbInitializer.InitializeAsync`, so callers receive a silent no-op.

**Touch points**

- `src/Genocs.Persistence.EFCore/Initialization/ApplicationDbSeeder.cs`

**Acceptance criteria**

- Either: the commented-out seeding logic is restored and made functional (requires `IMultiTenantContext<GNXTenantInfo>` and, optionally, ASP.NET Core Identity integration).
- Or: the placeholder methods are removed and `ApplicationDbSeeder` is reduced to a thin coordinator that delegates entirely to `CustomSeederRunner`, with a clear contract document for what consumer seeders must provide.
- `AssignPermissionsToRoleAsync` signature is corrected regardless of chosen path.
- No `await Task.CompletedTask` placeholders remain in production paths.

**Dependencies**

- EFCORE-001 (tenant scope is now set correctly before `ApplicationDbSeeder` runs)

**Implementation notes**

- Removed all placeholder methods and commented-out identity seeding scaffolding from `src/Genocs.Persistence.EFCore/Initialization/ApplicationDbSeeder.cs`:
  - removed `SeedRolesAsync(...)`
  - removed `AssignPermissionsToRoleAsync(...)`
  - removed `SeedAdminUserAsync()`
- Reduced `ApplicationDbSeeder` to a thin coordinator that logs and delegates exclusively to `CustomSeederRunner.RunSeedersAsync(...)`.
- Documented the intended extension contract in code via method XML summary, pointing consumers to `ICustomSeeder` implementations for seeding behavior.
- Updated `ApplicationDbInitializer.InitializeAsync(...)` to actually invoke seeding through `ApplicationDbSeeder.SeedDatabaseAsync(...)` in successful initialization paths:
  - MongoDB path (no migrations)
  - no-pending-migrations path
  - post-migration path
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -c Debug --nologo` (22/22 passing).

---

### EFCORE-012 Remove hardcoded default password constant

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

`MultitenancyConstants.DefaultPassword = "123Pa$$word!"` is a public constant compiled into the NuGet package. Publishing known credentials in a library constant violates OWASP A07 (Identification and Authentication Failures).

**Touch points**

- `src/Genocs.Persistence.EFCore/Multitenancy/MultitenancyConstants.cs`

**Acceptance criteria**

- `DefaultPassword` constant is removed.
- Any internal usage is replaced by caller-supplied or configuration-sourced values.
- No hardcoded credential of any kind remains in the package.

**Dependencies**

- EFCORE-011 (seeder currently references the constant in commented-out code)

**Implementation notes**

- Removed `MultitenancyConstants.DefaultPassword` from `src/Genocs.Persistence.EFCore/Multitenancy/MultitenancyConstants.cs`.
- Verified no remaining usages of `MultitenancyConstants.DefaultPassword` inside `Genocs.Persistence.EFCore` package source.
- This removes hardcoded credential material from the package and aligns seeding customization with the `ICustomSeeder` contract introduced via EFCORE-011.
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -c Debug --nologo`.

---

### EFCORE-013 Remove `AppContext.SetSwitch` from `TenantDbContext` constructor

**Status**: Not started

**Priority**: P1

**Problem**

`TenantDbContext..ctor` calls `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` unconditionally. This sets a process-wide flag on every `TenantDbContext` construction (which can occur multiple times per request) regardless of whether PostgreSQL is even in use. Since the tenant store is now pinned to SQL Server, the Npgsql switch is entirely vestigial.

**Touch points**

- `src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantDbContext.cs`

**Acceptance criteria**

- `AppContext.SetSwitch` is removed from the constructor.
- If legacy timestamp behavior is genuinely required for PostgreSQL deployments, it is documented as a startup configuration responsibility for the consuming application, not set by the library.

**Dependencies**

- none

---

### EFCORE-014 Replace deprecated `GetQueryFilter()` with `GetDeclaredQueryFilters()` on net10

**Status**: Not started

**Priority**: P1

**Problem**

`AppendGlobalQueryFilterExtension.AppendGlobalQueryFilter` calls `Metadata.GetQueryFilter()`, which is marked `[Obsolete]` in EF Core 10. The build emits `CS0618` on `net10.0`.

**Touch points**

- `src/Genocs.Persistence.EFCore/Context/AppendGlobalQueryFilterExtension.cs`

**Acceptance criteria**

- `GetQueryFilter()` replaced with `GetDeclaredQueryFilters()` under a `#if NET10_0_OR_GREATER` guard, preserving existing behavior for `net8.0` and `net9.0`.
- `CS0618` warning on `net10.0` is resolved.

**Dependencies**

- none

---

### EFCORE-015 Fix `ConnectionStringValidator` SQLite validation to use string parsing only

**Status**: Not started

**Priority**: P1

**Problem**

`ConnectionStringValidator.TryValidate` validates an SQLite connection string by constructing a `SqliteConnection` object, which acquires OS-level resources. Validation should only parse the string, not open a connection. Oracle validation is also commented out with no implementation path.

Since the provider split, connection string validation and securing live in each provider package behind `IEFCoreDbProvider`; the SQLite behavior was preserved as-is (`SqliteDbProvider` still constructs a `SqliteConnection`, and its `MakeSecureConnectionString` returns `builder.ToString()` on the connection object — the type name — rather than the connection string).

**Touch points**

- `src/Genocs.Persistence.EFCore.Sqlite/SqliteDbProvider.cs`
- `src/Genocs.Persistence.EFCore.Oracle/OracleDbProvider.cs`

**Acceptance criteria**

- SQLite validation uses `SqliteConnectionStringBuilder` to parse and validate the string without creating a connection object.
- `SqliteDbProvider.MakeSecureConnectionString` returns the actual connection string instead of `SqliteConnection.ToString()`.
- Oracle validation is either implemented using `OracleConnectionStringBuilder` or explicitly removed with a comment explaining the omission.
- No connection-opening side effects occur during validation.

**Dependencies**

- none

---

## M3: Testability and Quality Gates

### EFCORE-016 Create unit test project for repository and registration contracts

**Status**: Not started

**Priority**: P2

**Problem**

No test project exists for `Genocs.Persistence.EFCore`. Critical behaviors — repository registration, migration gating, multi-tenancy scope injection, event decorator — are entirely unprotected by automated tests.

**Touch points**

- `src/tests/Genocs.Persistence.EFCore.UnitTests/` (new project)

**Acceptance criteria**

- Unit test project created using xUnit, targeting `net8.0` minimum.
- Tests cover:
  - `ApplicationDbInitializer`: no-pending-migrations path, auto-apply path, halt path.
  - `DatabaseInitializer.InitializeApplicationDbForTenantAsync`: tenant scope injection, dedicated connection string override, shared connection fallback.
  - `AddRepositories`: correct assembly scanning behavior after EFCORE-003 fix.
  - `EventAddingRepositoryDecorator`: domain event added on Add/Update/Delete.
- Test run reports non-zero executed tests, all passing.

**Dependencies**

- EFCORE-001, EFCORE-002, EFCORE-003

---

### EFCORE-017 Replace `Activator.CreateInstance` decorator wiring with proper DI factory

**Status**: Not started

**Priority**: P2

**Problem**

`EventAddingRepositoryDecorator<T>` is instantiated via `Activator.CreateInstance` in the `AddRepositories` loop, bypassing the DI container. Any constructor signature change causes a silent runtime failure rather than a startup error.

**Touch points**

- `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`

**Acceptance criteria**

- `Activator.CreateInstance` is removed from the registration loop.
- `EventAddingRepositoryDecorator<T>` is wired using a typed factory lambda that resolves all dependencies through `IServiceProvider`.
- Constructor changes to `EventAddingRepositoryDecorator<T>` fail at startup (DI resolution time) rather than at first use.

**Dependencies**

- EFCORE-003

---

### EFCORE-018 Consolidate duplicate `UseDatabase` extension method

**Status**: Completed (July 2026, superseded by the provider split)

**Priority**: P2

**Problem**

Two `internal static DbContextOptionsBuilder UseDatabase(...)` methods exist — one in `EFCoreExtensions.cs` and one in `Multitenancy/Extensions.cs`. They are nearly identical but diverge on MongoDB handling and conditional compilation guards. Changes to one do not propagate to the other.

**Touch points**

- `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`
- `src/Genocs.Persistence.EFCore/Multitenancy/Extensions.cs`

**Acceptance criteria**

- A single `UseDatabase` implementation exists in one location (recommend `EFCoreExtensions.cs`).
- `Multitenancy/Extensions.cs` calls the shared implementation.
- Both database setup paths (app context and tenant context) behave consistently.

**Dependencies**

- EFCORE-005 (MongoDB path must be correct before consolidation)

**Implementation notes**

- Both `UseDatabase` switches were deleted rather than consolidated: DbContext configuration is now dispatched through `IEFCoreDbProvider.Configure(...)`, resolved from DI by provider key.
- `AddEFCorePersistence` resolves the provider for `ApplicationDbContext`; the multitenancy package pins `TenantDbContext` to `SqlServerDbProvider` directly.
- The `#if !NET10_0_OR_GREATER` MySQL guards moved out of shared code into `Genocs.Persistence.EFCore.MySql`, where `Configure` throws `NotSupportedException` on net10 until Pomelo supports EF Core 10.
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -v q --nologo` (33/33 passing).

---

### EFCORE-019 Remove `Serilog.Sinks.MSSqlServer` from the EFCore package

**Status**: Completed (July 2026)

**Priority**: P2

**Problem**

`Serilog.Sinks.MSSqlServer` is a logging infrastructure dependency with no usage inside the EFCore package. It forces every consumer to carry the SQL Server Serilog sink.

**Touch points**

- `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj`

**Acceptance criteria**

- `Serilog.Sinks.MSSqlServer` package reference is removed.
- Build succeeds and no code in the package references the sink.

**Dependencies**

- none

**Implementation notes**

- Removed the `Serilog.Sinks.MSSqlServer` package reference during the provider split; the reference was unused. `Serilog.Extensions.Hosting` remains for the static logger used by `EFCoreExtensions`.
- Validation: `dotnet build src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj -v q --nologo`.

---

## M4: Polishing, Documentation, and Release Readiness

### EFCORE-020 Replace bare `Exception` in `GNXTenantInfo.SetValidity` with a domain exception

**Status**: Not started

**Priority**: P2

**Problem**

`SetValidity` throws `new Exception("Subscription cannot be backdated.")` — a bare `System.Exception` from a domain method.

**Touch points**

- `src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/GNXTenantInfo.cs`

**Acceptance criteria**

- `throw new Exception(...)` replaced with `throw new InvalidOperationException(...)` or a custom domain exception type.

**Dependencies**

- none

---

### EFCORE-021 Consolidate duplicate `DbProviderKeys` class

**Status**: Completed (July 2026)

**Priority**: P2

**Problem**

`DbProviderKeys` is defined in both `Genocs.Persistence.EFCore.Common` and `Genocs.Persistence.EFCore.MultiTenancy`. Both are `internal` and contain identical constants.

**Touch points**

- `src/Genocs.Persistence.EFCore/Common/DbProviderKeys.cs`
- `src/Genocs.Persistence.EFCore/Multitenancy/DbProviderKeys.cs`

**Acceptance criteria**

- One `DbProviderKeys` class remains (keep `Common`).
- The `MultiTenancy` duplicate is deleted.
- All usages in `Multitenancy/` reference the `Common` class.

**Dependencies**

- EFCORE-018 (consolidation is cleaner after `UseDatabase` deduplication)

**Implementation notes**

- Deleted the `Multitenancy/DbProviderKeys.cs` duplicate; the single `Genocs.Persistence.EFCore.Common.DbProviderKeys` class remains and was made `public static` so provider packages can reference the keys.
- Validation: `dotnet test src/tests/Genocs.Persistence.EFCore.UnitTests/Genocs.Persistence.EFCore.UnitTests.csproj -v q --nologo` (33/33 passing).

---

### EFCORE-022 Remove `IDbConnection Connection` from `ApplicationDbContext`

**Status**: Not started

**Priority**: P2

**Problem**

`ApplicationDbContext` exposes `public IDbConnection Connection => Database.GetDbConnection()` to support `DapperRepository`, which is entirely commented out. The raw connection property has no guarded usage and creates a SQL injection surface.

**Touch points**

- `src/Genocs.Persistence.EFCore/Context/ApplicationDbContext.cs`

**Acceptance criteria**

- `IDbConnection Connection` property is removed.
- If Dapper is implemented (see EFCORE-008), the property is restored only within a guarded Dapper-specific context subclass or extension.

**Dependencies**

- EFCORE-008

---

### EFCORE-023 Fix nullable reference return in `TenantService.GetByIdAsync`

**Status**: Not started

**Priority**: P2

**Problem**

`TenantService.GetByIdAsync` returns `TenantDto` (non-nullable) but `Adapt<TenantDto>()` can produce null, causing compiler warning `CS8603`.

**Touch points**

- `src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantService.cs`

**Acceptance criteria**

- Return value is guarded with a null check; if null, a `NotFoundException` is thrown.
- `CS8603` warning is resolved.

**Dependencies**

- none

---

### EFCORE-024 Remove hardcoded root email placeholder from `MultitenancyConstants`

**Status**: Not started

**Priority**: P2

**Problem**

`MultitenancyConstants.Root.EmailAddress = "admin@root.com"` is a public constant with a `// TODO` comment. Root tenant configuration should be supplied through options.

**Touch points**

- `src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/MultitenancyConstants.cs`

**Acceptance criteria**

- `EmailAddress` constant is removed from `MultitenancyConstants.Root`.
- Any internal usage is sourced from configuration or constructor injection.

**Dependencies**

- EFCORE-012

---

### EFCORE-025 Expand README with provider setup, migration, and multi-tenancy guidance

**Status**: Not started

**Priority**: P2

**Problem**

`README_NUGET.md` contains only a package description and the `AddEFCorePersistence` entry point. Consumers have no guidance on provider configuration, migration assembly requirements, multi-tenancy setup, or known limitations.

**Touch points**

- `src/Genocs.Persistence.EFCore/README_NUGET.md`

**Acceptance criteria**

- README documents all supported `DBProvider` values (`mssql`, `postgresql`, `mysql`, `oracle`, `sqlite`, `mongodb`).
- Migration assembly naming convention (`Migrators.MSSQL`, etc.) is documented.
- `AutoApplyMigrations` flag behavior is documented with a production warning.
- Multi-tenancy setup using `AddFinbuckleMultiTenancyWithEfCoreStore` is documented.
- Known limitations are listed (no MySQL on net10, MongoDB name must be configured, Dapper not currently supported).
- `CHANGELOG.md` is updated under `Unreleased` to record behavior changes from M1–M3.

**Dependencies**

- EFCORE-001 through EFCORE-024

---

## Definition of Done (Backlog Level)

- All P0 tasks are completed and validated.
- Package contains no `NotImplementedException` paths in public operational flows.
- Repository registration correctly discovers consumer-defined aggregate root types.
- Multi-tenancy EF Core store path is reachable and functional.
- Test baseline is executable with non-zero passing tests.
- Consumer documentation reflects current supported behavior.
