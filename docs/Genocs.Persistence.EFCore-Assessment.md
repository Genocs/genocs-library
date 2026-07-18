# Genocs.Persistence.EFCore — Assessment

> **Note (July 2026)**: This assessment predates the package split. `Genocs.Persistence.EFCore` has since been divided into a provider-agnostic core, six database provider packages (`Genocs.Persistence.EFCore.SqlServer`, `.PostgreSQL`, `.MySql`, `.Sqlite`, `.Oracle`, `.MongoDB`), and `Genocs.Persistence.EFCore.MultiTenancy.SqlServer`. File paths and dependency observations below reflect the pre-split layout. For current state see [Genocs.Persistence.EFCore-Agent-Documentation.md](Genocs.Persistence.EFCore-Agent-Documentation.md) and the [implementation backlog](Genocs.Persistence.EFCore-Implementation-Backlog.md).

## Overview

This assessment covers the `Genocs.Persistence.EFCore` package as of May 2026. The analysis examines runtime correctness, DI composition, repository contracts, multi-tenancy behavior, dependency hygiene, and testability.

**Build baseline**: `dotnet build src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj -c Debug --nologo`  
**Result**: Builds successfully for `net8.0`, `net9.0`, and `net10.0` with 5–6 warnings per target framework.  
**Test coverage**: None. No unit test project or integration test project exists for this package.

---

## Current State Summary

| Area | Status |
|---|---|
| Build | Succeeds with warnings |
| Runtime correctness | Broken — multiple paths throw `NotImplementedException` or are no-ops |
| Repository registration | Broken — scans wrong assembly |
| Database initialization | Broken — migrations detected but never applied |
| Multi-tenancy | Partially implemented — unreachable code, `NotImplementedException` on tenant creation |
| DI composition | Moderate — uses obsolete interfaces, `Activator.CreateInstance` for decorator wiring |
| Dependency hygiene | Poor — MongoDB driver in an EFCore package, Serilog SQL sink coupled to persistence layer |
| Testability | Zero — no test projects exist |
| Documentation | Minimal — README has no usage, migration, or multi-tenancy guidance |

---

## P0 — Critical: Runtime Failures

### EFCORE-001 `InitializeApplicationDbForTenantAsync` throws `NotImplementedException`

**File**: `src/Genocs.Persistence.EFCore/Initialization/DatabaseInitializer.cs`

The `IDatabaseInitializer.InitializeApplicationDbForTenantAsync(GNXTenantInfo, CancellationToken)` implementation throws `NotImplementedException`. This method is called from `TenantService.CreateAsync`, which means any attempt to create a new tenant causes a runtime crash.

```csharp
public Task InitializeApplicationDbForTenantAsync(GNXTenantInfo tenant, CancellationToken cancellationToken)
{
    throw new NotImplementedException(); // Called from TenantService.CreateAsync
}
```

**Impact**: Tenant provisioning is completely non-functional at runtime.

---

### EFCORE-002 `ApplicationDbInitializer.InitializeAsync` never applies migrations

**File**: `src/Genocs.Persistence.EFCore/Initialization/ApplicationDbInitializer.cs`

The initializer detects pending migrations but never applies them. The body ends with `await Task.CompletedTask` instead of calling `Database.MigrateAsync(cancellationToken)`.

```csharp
if (_dbContext.Database.GetMigrations().Any())
{
    _logger.LogInformation("Find migrations that need to be apply...");
}
await Task.CompletedTask; // Never migrates
```

**Impact**: Calling `InitializeDatabasesAsync` produces no schema changes. Consumers relying on auto-migration on startup will have no schema.

---

### EFCORE-003 `AddRepositories` scans the wrong assembly

**File**: `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`

The repository auto-registration loop iterates `typeof(IAggregateRoot).Assembly`, which resolves to the `Genocs.Common` assembly — not the consuming application's assembly. Consumer aggregate root types are never found, so no application-level repositories are registered.

```csharp
foreach (var aggregateRootType in
    typeof(IAggregateRoot).Assembly.GetExportedTypes()  // Always Genocs.Common — not the app
        .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
```

**Impact**: `IRepository<T>`, `IReadRepository<T>`, and `IRepositoryWithEvents<T>` are never registered for any consumer entity type, causing `InvalidOperationException` on first use.

---

### EFCORE-004 Unreachable code disables the EF Core tenant store path

**File**: `src/Genocs.Persistence.EFCore/Multitenancy/Extensions.cs` (line 68)

The `AddFinbuckleMultiTenancy<TTenantInfo>` overload contains an early `return services;` followed by more configuration code that can never execute. The second block — which registers `TenantDbContext`, multi-tenancy strategies, and `ITenantService` — is unreachable dead code.

```csharp
return services;          // Returns here — everything below is dead code (CS0162)

return services
    .AddDbContext<TenantDbContext>(...)
    .AddMultiTenant<GNXTenantInfo>()
    .WithEFCoreStore<TenantDbContext, GNXTenantInfo>()
    ...
    .AddScoped<ITenantService, TenantService>(); // Never registered
```

**Impact**: `ITenantService` is never registered. All multi-tenancy EF Core store wiring is silently skipped. The compiler emits warning `CS0162` on all targets.

---

### EFCORE-005 MongoDB `UseDatabase` uses hardcoded `"DatabaseName"` literal

**File**: `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`

When `DBProvider` is set to `mongodb`, the `UseDatabase` method calls:

```csharp
DbProviderKeys.MongoDB => builder.UseMongoDB(connectionString, "DatabaseName"),
```

The database name is hardcoded to the string `"DatabaseName"` rather than a configured value. Every MongoDB-backed consumer will connect to the same literal database, regardless of what is configured.

**Impact**: MongoDB provider path is functionally broken for any real deployment.

---

## P1 — High: Design and Safety Issues

### EFCORE-006 `DomainEventExtensions.AddDomainEvent` uses reflection to access a private field

**File**: `src/Genocs.Persistence.EFCore/Repositories/DomainEventExtensions.cs`

The method adds domain events to aggregate roots by reflecting on a private `_domainEvents` field:

```csharp
var field = aggregate.GetType().GetField("_domainEvents",
    BindingFlags.NonPublic | BindingFlags.Instance);
if (field?.GetValue(aggregate) is List<IEvent> list)
    list.Add(@event);
else
    throw new InvalidOperationException("Cannot add domain event: backing field not found...");
```

**Concerns**:
- Breaks at runtime if any entity's backing field is named differently.
- Bypasses type safety — there is no compile-time contract enforcing the field exists.
- Throws an uninformative `InvalidOperationException` for the consumer.
- The `IGeneratesDomainEvents` interface should expose an `AddDomainEvent` method; if it does not, the interface should be extended.

---

### EFCORE-007 `MongoDB.Driver` directly referenced in an EFCore package

**File**: `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj`

`MongoDB.Driver` is listed as a direct (unconditional) package reference:

```xml
<PackageReference Include="MongoDB.Driver" Version="3.8.0" />
```

This was introduced so that `ConnectionStringSecurer` can parse MongoDB connection strings. An EFCore package forces every consumer — even those using only SQL Server — to carry the full MongoDB driver. The MongoDB driver is a large dependency with its own BSON serialization infrastructure.

**Fix**: Extract MongoDB connection string parsing behind an interface or conditional compilation. Alternatively, parse MongoDB URIs using `UriBuilder` without the MongoDB driver.

---

### EFCORE-008 `DapperRepository` is entirely commented out, creating dead code

**File**: `src/Genocs.Persistence.EFCore/Repositories/DapperRepository.cs`

The entire class is wrapped in a block comment. The `IDbConnection Connection` property is still exposed on `ApplicationDbContext` and namespace imports are present, but no Dapper package reference exists. The result is confusing dead code with no path to functionality.

**Fix**: Either implement Dapper support as a separate opt-in extension (with a `Dapper` package reference), or remove the file entirely.

---

### EFCORE-009 `ITransientService` and `IScopedService` are deprecated

**File**: `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`

Two `CS0618` warnings on all target frameworks:

```
warning CS0618: 'ITransientService' is obsolete: 'Use ITransientDependency from Genocs.Common.Dependency instead.'
warning CS0618: 'IScopedService' is obsolete: 'Use IScopedDependency from Genocs.Common.Dependency instead.'
```

These interfaces should be replaced with their current `ITransientDependency` and `IScopedDependency` equivalents.

---

### EFCORE-010 `Genocs.Core` version in release build references pre-release beta

**File**: `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj`

```xml
<ItemGroup Condition="'$(Configuration)' == 'Release'">
    <PackageReference Include="Genocs.Core" Version="9.0.0-beta007" />
</ItemGroup>
```

A pre-release `beta` version as the release dependency is unstable. The version also does not match the package's own supported target frameworks (`net10.0`, `net9.0`, `net8.0`). Version `9.0.0` suggests a net9 lineage, not net10.

---

### EFCORE-011 `ApplicationDbSeeder` is entirely non-functional placeholder code

**File**: `src/Genocs.Persistence.EFCore/Initialization/ApplicationDbSeeder.cs`

All three seeder methods (`SeedRolesAsync`, `AssignPermissionsToRoleAsync`, `SeedAdminUserAsync`) consist entirely of commented-out code followed by `await Task.CompletedTask`. No actual seeding happens. The method `AssignPermissionsToRoleAsync` also has a wrong signature — its parameters were stripped when the body was commented out.

The class is called from `ApplicationDbInitializer`, so callers expect seeding to occur but receive a no-op instead.

---

### EFCORE-012 Hardcoded default password in a public constant

**File**: `src/Genocs.Persistence.EFCore/Multitenancy/MultitenancyConstants.cs`

```csharp
public const string DefaultPassword = "123Pa$$word!";
```

A hardcoded credential in a public constant ships inside a NuGet package and is visible to all consumers. Even with a "development only" intent, shipping known passwords in a public library constant is a security concern (OWASP A07: Identification and Authentication Failures).

**Fix**: Remove the constant. Require callers to supply or generate credentials through configuration.

---

### EFCORE-013 `AppContext.SetSwitch` called from `TenantDbContext` constructor

**File**: `src/Genocs.Persistence.EFCore/Multitenancy/TenantDbContext.cs`

```csharp
public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
{
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}
```

Setting a process-wide switch from an object constructor is unsafe:
- The switch applies globally regardless of which database provider is actually used.
- The behavior is set every time a `TenantDbContext` is constructed, which may happen multiple times per request.
- Consumers running non-legacy PostgreSQL setups will have their timestamp behavior silently overridden.

---

### EFCORE-014 `IReadOnlyEntityType.GetQueryFilter()` is obsolete on net10

**File**: `src/Genocs.Persistence.EFCore/Context/AppendGlobalQueryFilterExtension.cs`

On `net10.0` the build emits:

```
warning CS0618: 'IReadOnlyEntityType.GetQueryFilter()' is obsolete: 'Use GetDeclaredQueryFilters() instead.'
```

---

### EFCORE-015 `ConnectionStringValidator` creates `SqliteConnection` object for validation

**File**: `src/Genocs.Persistence.EFCore/Configurations/ConnectionStringValidator.cs`

To validate an SQLite connection string, the validator creates an actual `SqliteConnection` object rather than just parsing it:

```csharp
case DbProviderKeys.SqLite:
    var sqlite = new SqliteConnection(connectionString);
    break;
```

`SqliteConnection` acquires OS-level resources and this approach conflates parsing with opening. Also, Oracle validation is commented out with no path to implementation.

---

## P2 — Medium: Quality and Maintainability

### EFCORE-016 No unit test or integration test project

There is no test project for `Genocs.Persistence.EFCore`. All other library packages have corresponding test projects. This leaves the following untested:

- Repository registration and resolution
- Database initialization and migration application
- Multi-tenancy wiring
- Event decorator behavior
- Connection string parsing and securing

---

### EFCORE-017 `EventAddingRepositoryDecorator` wired via `Activator.CreateInstance`

**File**: `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs`

```csharp
services.AddScoped(typeof(IRepositoryWithEvents<>).MakeGenericType(aggregateRootType), sp =>
    Activator.CreateInstance(
        typeof(EventAddingRepositoryDecorator<>).MakeGenericType(aggregateRootType),
        sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)))
    ?? throw new InvalidOperationException(...));
```

Using `Activator.CreateInstance` bypasses DI entirely. If `EventAddingRepositoryDecorator`'s constructor signature changes, the failure is at runtime, not at compile time. A proper `sp.GetRequiredService<>` factory or an open-generic registration is preferred.

---

### EFCORE-018 Duplicate `UseDatabase` extension method

**File**: `src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs` and `src/Genocs.Persistence.EFCore/Multitenancy/Extensions.cs`

Two `internal static DbContextOptionsBuilder UseDatabase(...)` extension methods exist, one in each file. They are nearly identical but subtly different (the multitenancy version omits MongoDB and has different conditional compilation structure). This duplication creates a maintenance hazard where updating one does not update the other.

---

### EFCORE-019 `Serilog.Sinks.MSSqlServer` dependency couples persistence to logging infrastructure

**File**: `src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj`

```xml
<PackageReference Include="Serilog.Sinks.MSSqlServer" Version="9.0.3" />
```

A persistence library package should not own a Serilog SQL Server sink dependency. Logging sinks belong in the `Genocs.Logging` package. This forces any EFCore consumer to carry the SQL Server sink regardless of their logging setup.

---

### EFCORE-020 `GNXTenantInfo.SetValidity` throws bare `Exception`

**File**: `src/Genocs.Persistence.EFCore/Multitenancy/GNXTenantInfo.cs`

```csharp
public void SetValidity(in DateTime validTill) =>
    ValidUpTo = ValidUpTo < validTill
        ? validTill
        : throw new Exception("Subscription cannot be backdated.");
```

A domain method should throw a domain-specific or documented exception type, not `System.Exception`. `InvalidOperationException` or a custom domain exception is appropriate here.

---

### EFCORE-021 `DbProviderKeys` is internal but duplicated

**File**: `src/Genocs.Persistence.EFCore/Common/DbProviderKeys.cs` and `src/Genocs.Persistence.EFCore/Multitenancy/DbProviderKeys.cs`

Two `DbProviderKeys` classes exist — one in `Genocs.Persistence.EFCore.Common` and one in `Genocs.Persistence.EFCore.MultiTenancy`. Both define the same provider key constants. This duplication is a maintenance hazard.

---

### EFCORE-022 `ApplicationDbContext` exposes `IDbConnection` but Dapper is not supported

**File**: `src/Genocs.Persistence.EFCore/Context/ApplicationDbContext.cs`

```csharp
public IDbConnection Connection => Database.GetDbConnection();
```

The property exists to support `DapperRepository`, which is entirely commented out and has no package dependency. Exposing raw `IDbConnection` from the public `DbContext` type creates a surface area for SQL injection and raw query abuse with no supported path to correct usage.

---

### EFCORE-023 `TenantService.GetByIdAsync` has a possible null reference return

**File**: `src/Genocs.Persistence.EFCore/Multitenancy/TenantService.cs` (line 71)

```
warning CS8603: Possible null reference return.
```

`Adapt<TenantDto>()` from Mapster may return null if the tenant object is null after deserialization. The return type is non-nullable `TenantDto`.

---

### EFCORE-024 `MultitenancyConstants.Root.EmailAddress` is a public hardcoded placeholder

**File**: `src/Genocs.Persistence.EFCore/Multitenancy/MultitenancyConstants.cs`

```csharp
public const string EmailAddress = "admin@root.com"; // TODO: Change the email address.
```

A hardcoded email with a `TODO` comment is shipping in a public library constant. Root tenant configuration should be supplied via options, not compiled into the library.

---

### EFCORE-025 README provides no meaningful guidance

**File**: `src/Genocs.Persistence.EFCore/README_NUGET.md`

The README lists only the `AddEFCorePersistence` entry point with no information about:
- Supported provider configuration (`DBProvider` values)
- Migration assembly requirements (`Migrators.MSSQL`, etc.)
- Multi-tenancy setup
- Known limitations (no MySQL on net10, no MongoDB database name, no Dapper)
- Breaking behavior notes

---

## Dependency Inventory

| Package | Version | Concern |
|---|---|---|
| `MongoDB.Driver` | `3.8.0` | Not EFCore-related; should not be a direct dependency |
| `Serilog.Sinks.MSSqlServer` | `9.0.3` | Logging infrastructure; belongs in Genocs.Logging |
| `Finbuckle.MultiTenant.AspNetCore` | `9.4.3` | Pulls ASP.NET Core into a persistence package |
| `Finbuckle.MultiTenant.EntityFrameworkCore` | `9.4.1` | Acceptable for EFCore multi-tenancy |
| `Mapster` | `7.4.0` | Used in repository projection and DTO mapping |
| `MediatR` | `14.1.0` | Used only in `GetAuditLogsRequestHandler`; consider moving to application layer |
| `Ardalis.Specification.EntityFrameworkCore` | `9.3.1` | Core EFCore repository abstraction |

---

## Issue Summary

| ID | Title | Priority | Area |
|---|---|---|---|
| EFCORE-001 | `InitializeApplicationDbForTenantAsync` throws `NotImplementedException` | P0 | Runtime |
| EFCORE-002 | `ApplicationDbInitializer` never applies migrations | P0 | Runtime |
| EFCORE-003 | `AddRepositories` scans wrong assembly | P0 | DI / Registration |
| EFCORE-004 | Unreachable code disables EF Core tenant store path | P0 | Multi-tenancy |
| EFCORE-005 | MongoDB `UseDatabase` hardcodes `"DatabaseName"` literal | P0 | Provider Config |
| EFCORE-006 | `DomainEventExtensions` uses reflection on private field | P1 | Design |
| EFCORE-007 | `MongoDB.Driver` is a direct dependency in an EFCore package | P1 | Dependencies |
| EFCORE-008 | `DapperRepository` is entirely commented-out dead code | P1 | Design |
| EFCORE-009 | Deprecated `ITransientService`/`IScopedService` usage | P1 | API Hygiene |
| EFCORE-010 | Release build references pre-release beta `Genocs.Core` | P1 | Dependencies |
| EFCORE-011 | `ApplicationDbSeeder` is entirely non-functional placeholder | P1 | Runtime |
| EFCORE-012 | Hardcoded default password in public constant | P1 | Security |
| EFCORE-013 | `AppContext.SetSwitch` called from `TenantDbContext` constructor | P1 | Safety |
| EFCORE-014 | `GetQueryFilter()` deprecated on net10 | P1 | API Hygiene |
| EFCORE-015 | `ConnectionStringValidator` creates `SqliteConnection` for validation | P1 | Safety |
| EFCORE-016 | No unit or integration test project | P2 | Testability |
| EFCORE-017 | Decorator wired via `Activator.CreateInstance` | P2 | DI |
| EFCORE-018 | Duplicate `UseDatabase` extension method | P2 | Maintainability |
| EFCORE-019 | `Serilog.Sinks.MSSqlServer` in persistence package | P2 | Dependencies |
| EFCORE-020 | `SetValidity` throws bare `Exception` | P2 | Design |
| EFCORE-021 | `DbProviderKeys` duplicated across namespaces | P2 | Maintainability |
| EFCORE-022 | `IDbConnection` exposed on context without Dapper support | P2 | Design |
| EFCORE-023 | Nullable reference return in `TenantService.GetByIdAsync` | P2 | Nullability |
| EFCORE-024 | Hardcoded root email placeholder in public constant | P2 | Design |
| EFCORE-025 | README provides no meaningful guidance | P2 | Documentation |

---

## Suggested Milestone Structure

| Milestone | Goal | Issues |
|---|---|---|
| M1 | Runtime correctness baseline | EFCORE-001 to EFCORE-005 |
| M2 | DI, registration, and dependency hygiene | EFCORE-006 to EFCORE-015 |
| M3 | Testability and quality gates | EFCORE-016 to EFCORE-019 |
| M4 | Polishing, documentation, and release readiness | EFCORE-020 to EFCORE-025 |
