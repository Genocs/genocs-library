# Plan: upgrade Finbuckle to 10.1.5 in `Genocs.Persistence.EFCore.MultiTenancy.SqlServer`

Implementation brief for an agent working in the **genocs-library** repository. Do the upgrade in this repo. Do not fork Finbuckle and do not retarget the rest of the library.

## Goal

Move `Genocs.Persistence.EFCore.MultiTenancy.SqlServer` from Finbuckle **9.4.12** to **10.1.5** (released 2026-10-19), the current latest of:

- `Finbuckle.MultiTenant.AspNetCore`
- `Finbuckle.MultiTenant.EntityFrameworkCore` (pulls `Finbuckle.MultiTenant` 10.1.5)

Genocs registration methods stay the same for callers:

- `AddFinbuckleMultiTenancy<TTenantInfo>`
- `AddFinbuckleMultiTenancyWithEfCoreStore` (both overloads)
- `UseMultiTenancy`
- `ITenantService` / `ITenantDatabaseInitializer`

## Decision

Take **10.1.5** and retarget this package to **`net10.0` only**.

Finbuckle 10.0.0 removed the `net8.0` and `net9.0` targets. 10.1.5 is `net10.0` only (`Microsoft.EntityFrameworkCore.Relational` 10.0.13). The package cannot keep `net10.0;net9.0;net8.0` and reference 10.1.5.

`Genocs.Persistence.EFCore` and `Genocs.Persistence.EFCore.SqlServer` stay multi-targeted. A `net10.0` project can still reference them. In-repo consumers are already `net10.0`:

- `tests/Persistence.EFCore.UnitTests`
- `src/apps/integrations/WebApi`
- `src/apps/integrations/Masstransit.WebApi`

Do not bump the shared `<Version>` in `src/Directory.Build.props`. Call the TFM drop out as a breaking change for this package only in `CHANGELOG.md`.

The latest 9.x patch is **9.4.14** (2026-10-18). Use it only if this package must keep shipping `net8.0` and `net9.0`. That patch does not need the code changes below. This plan does not implement 9.4.14.

## Current behavior

Package file: `src/Persistence.EFCore.MultiTenancy.SqlServer/Persistence.EFCore.MultiTenancy.SqlServer.csproj`

- `TargetFrameworks`: `net10.0;net9.0;net8.0`
- `Finbuckle.MultiTenant.AspNetCore` 9.4.12
- `Finbuckle.MultiTenant.EntityFrameworkCore` 9.4.12

`Extensions.cs` registers host strategy + configuration store, or claim/header/query-string strategies + `WithEFCoreStore<TenantDbContext, GNXTenantInfo>()`. `TenantService` calls Finbuckle 9 store methods: `TryAddAsync`, `TryUpdateAsync`, `TryRemoveAsync`, `TryGetAsync`, `GetAllAsync`. `TenantDatabaseInitializer` assigns the ambient tenant with:

```csharp
contextSetter.MultiTenantContext = new MultiTenantContext<GNXTenantInfo> { TenantInfo = tenant };
```

`GNXTenantInfo` derives from `TenantInfo` and declares its own `ConnectionString`. `TenantDbContext` derives from `EFCoreStoreDbContext<GNXTenantInfo>`. This repo has no EF migration for the tenant store.

## Required behavior

### 1. Package references and TFM

In `Persistence.EFCore.MultiTenancy.SqlServer.csproj`:

- `TargetFramework` `net10.0` (singular). Remove `net9.0` and `net8.0` from this project only.
- Both Finbuckle packages at `10.1.5`.

In `src/Persistence.EFCore/Persistence.EFCore.csproj`, raise the **net10.0** `Microsoft.EntityFrameworkCore.Relational` reference from `10.0.10` to `10.0.13`. Finbuckle.EntityFrameworkCore 10.1.5 depends on 10.0.13, and `Genocs.Persistence.EFCore.SqlServer` already uses `Microsoft.EntityFrameworkCore.SqlServer` 10.0.13 on net10. Leave the net8 and net9 EF pins unchanged.

### 2. Namespace moves

Finbuckle 10 moved extension methods to `*.Extensions` namespaces and moved several types. Update usings. Do not wrap Finbuckle types in new Genocs facades.

| Finbuckle 9 location used here | Finbuckle 10.1.5 location |
|---|---|
| `AddMultiTenant`, `WithConfigurationStore`, `WithDelegateStrategy` in `Finbuckle.MultiTenant` | `Finbuckle.MultiTenant.Extensions` |
| `WithHostStrategy`, `WithClaimStrategy`, `WithHeaderStrategy`, `UseMultiTenant` | `Finbuckle.MultiTenant.AspNetCore.Extensions` |
| `WithEFCoreStore` | `Finbuckle.MultiTenant.EntityFrameworkCore.Extensions` |
| `EFCoreStoreDbContext` in `Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore` | `Finbuckle.MultiTenant.EntityFrameworkCore.Stores` |
| `TenantInfo`, `MultiTenantContext<T>`, `IMultiTenantContextSetter` | `Finbuckle.MultiTenant.Abstractions` |
| `MultiTenantBuilder<T>` | `Finbuckle.MultiTenant` (unchanged) |

`WithConfigurationStore(configuration, configurationStoreSection)` still exists. Keep the Genocs default section `Finbuckle:MultiTenant:Stores:ConfigurationStore`.

`WithEFCoreStore<TStoreDbContext, TTenantInfo>()` still has that type-argument order. Keep `.WithEFCoreStore<TenantDbContext, GNXTenantInfo>()` and `.WithEFCoreStore<TStoreDbContext, TTenantInfo>()`.

`WithDelegateStrategy(Func<object, Task<string?>>)` still exists. Keep the private `WithQueryStringStrategy` helper.

### 3. Store methods in `TenantService`

`IMultiTenantStore<T>` no longer has the `Try*` names. Map calls and keep the same success/failure behavior (return values are still `Task<bool>` / `Task<T?>`; this service already ignores the bool).

| Current call | 10.1.5 call | Lookup key |
|---|---|---|
| `TryGetAsync(id)` | `GetAsync(id)` | tenant **Id** |
| `TryAddAsync(tenant)` | `AddAsync(tenant)` | |
| `TryUpdateAsync(tenant)` | `UpdateAsync(tenant)` | |
| `TryRemoveAsync(request.Id)` | `RemoveAsync(request.Id)` | tenant **Identifier** |
| `GetAllAsync()` | `GetAllAsync()` | unchanged |

`GNXTenantInfo` sets `Identifier = id` in its constructor. `RemoveAsync` takes the identifier, which is why passing `request.Id` still matches. Do not split `Id` and `Identifier`.

`GetAsync` is by id. `ExistsWithIdAsync` and `GetTenantInfoAsync` must use `GetAsync`, not `GetByIdentifierAsync`.

The 10.1.5 store constraint is `where TTenantInfo : ITenantInfo` (no `class`, no `new()`). Keep the stricter Genocs constraints (`class, ITenantInfo, new()`) on the public extension methods.

### 4. Ambient tenant context

`MultiTenantContext<T>` has no parameterless constructor. `TenantInfo` is `init`. Replace the object initializer with:

```csharp
contextSetter.MultiTenantContext = new MultiTenantContext<GNXTenantInfo>(tenant);
```

Keep resolving `IMultiTenantContextSetter` inside the initialization scope. Do not mutate a context instance that the middleware already built.

### 5. `GNXTenantInfo` and `TenantInfo`

`TenantInfo` in 10.1.5 lives in `Finbuckle.MultiTenant.Abstractions` and is:

```csharp
public class TenantInfo : ITenantInfo
{
    public required string Id { get; init; }
    public required string Identifier { get; init; }
    public string? Name { get; init; }
}
```

There is no base `ConnectionString`. Keep the property already declared on `GNXTenantInfo`. That is the column EF should map; do not add a second connection-string property.

The parameterized constructor already assigns `Id`, `Identifier`, and `Name`. That satisfies `required` for `new GNXTenantInfo(...)`. Keep the parameterless constructor so EF Core can materialize the entity. `required` is a compile-time check; do not add `[SetsRequiredMembers]` unless a `new GNXTenantInfo()` call site fails to compile.

Do not change `AdminEmail`, `IsActive`, `ValidUpTo`, `Issuer`, or the activate/deactivate/validity methods.

### 6. What not to change

- Do not adopt per-tenant EF global query filters, `MultiTenantDbContext`, or `IsNotMultiTenant`. This package uses Finbuckle only as a tenant store plus resolution strategies. `ApplicationDbContext` stays a normal DbContext. Per-tenant databases are still selected by `TenantDatabaseInitializer` calling `Database.SetConnectionString`.
- Do not register `WithPerTenantAuthentication`, route strategy, or base-path strategy.
- Do not add an EF migration. This repo has none for `TenantDbContext`. Finbuckle 10 drops the old 64-character max length on tenant id in its store model. Existing `Tenants` rows still fit. A later consumer migration may propose widening `Id`; that is outside this change.
- Do not edit `docs/archived/**`.
- Do not retarget other projects to drop `net8.0` / `net9.0`.

## Files

| File | Change |
|---|---|
| `src/Persistence.EFCore.MultiTenancy.SqlServer/Persistence.EFCore.MultiTenancy.SqlServer.csproj` | `net10.0` only; Finbuckle 10.1.5 |
| `src/Persistence.EFCore/Persistence.EFCore.csproj` | net10 `Microsoft.EntityFrameworkCore.Relational` 10.0.13 |
| `src/Persistence.EFCore.MultiTenancy.SqlServer/Extensions.cs` | extension usings; store namespace |
| `src/Persistence.EFCore.MultiTenancy.SqlServer/TenantDbContext.cs` | `Finbuckle.MultiTenant.EntityFrameworkCore.Stores` |
| `src/Persistence.EFCore.MultiTenancy.SqlServer/GNXTenantInfo.cs` | `TenantInfo` from `Finbuckle.MultiTenant.Abstractions` |
| `src/Persistence.EFCore.MultiTenancy.SqlServer/TenantService.cs` | store method names |
| `src/Persistence.EFCore.MultiTenancy.SqlServer/TenantDatabaseInitializer.cs` | `MultiTenantContext` constructor |
| `src/Persistence.EFCore.MultiTenancy.SqlServer/README.md` | supported TFM is `net10.0` |
| `docs/agents/Genocs.Persistence.EFCore-Agent-Documentation.md` | multitenancy package TFM is `net10.0`; Finbuckle 10.1.5; say `net8.0`/`net9.0` apps cannot reference this package |
| `CHANGELOG.md` | breaking: this package is `net10.0` only because Finbuckle 10 dropped older TFMs |

`AddFinbuckleMultiTenancyRegistrationTests` should keep compiling against the same Genocs extension methods. No test rewrite unless a registration assertion fails.

The integrations host (`MultiTenancyExtensions`, `appsettings.json` configuration-store section) calls Genocs extensions only. Leave it unless the build fails.

## Verification

From the repo root:

```bash
dotnet build src/Persistence.EFCore.MultiTenancy.SqlServer/Persistence.EFCore.MultiTenancy.SqlServer.csproj
dotnet test tests/Persistence.EFCore.UnitTests/Persistence.EFCore.UnitTests.csproj
dotnet build src/apps/integrations/WebApi/Host.csproj
dotnet build src/Persistence.EFCore/Persistence.EFCore.csproj
```

Confirm the multitenancy project restores and compiles only for `net10.0`, and that `Genocs.Persistence.EFCore` still builds `net8.0`, `net9.0`, and `net10.0`.

Fix compile errors that come from the namespace and store-signature moves above. Do not paper over them with obsolete shims.

## Source checked for 10.1.5

- https://github.com/Finbuckle/Finbuckle.MultiTenant/releases/tag/v10.1.5
- `IMultiTenantStore` method names and identifier-vs-id remarks: `src/Finbuckle.MultiTenant.Abstractions/IMultiTenantStore.cs` at tag `v10.1.5`
- `TenantInfo` required init properties: `src/Finbuckle.MultiTenant.Abstractions/TenantInfo.cs`
- `MultiTenantContext` constructors: `src/Finbuckle.MultiTenant.Abstractions/MultiTenantContext.cs`
- `EFCoreStoreDbContext` namespace: `src/Finbuckle.MultiTenant.EntityFrameworkCore/Stores/EFCoreStoreDbContext.cs`
- `WithEFCoreStore`: `src/Finbuckle.MultiTenant.EntityFrameworkCore/Extensions/MultiTenantBuilderExtensions.cs`
- `WithConfigurationStore` / `WithDelegateStrategy`: `src/Finbuckle.MultiTenant/Extensions/MultiTenantBuilderExtensions.cs`
- ASP.NET Core strategies and `UseMultiTenant`: `src/Finbuckle.MultiTenant.AspNetCore/Extensions/`
- TFM: `net10.0` in the 10.1.5 csproj files; nuspec dependency `Microsoft.EntityFrameworkCore.Relational` 10.0.13
