# Genocs.Persistence.EFCore

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

EF Core repository and persistence integration for Genocs applications. Supports `net10.0`, `net9.0`, and `net8.0`.

This package is provider-agnostic and references no database driver. Install it together with the provider package matching your database:

| Provider package | `DBProvider` value |
|---|---|
| `Genocs.Persistence.EFCore.SqlServer` | `mssql` |
| `Genocs.Persistence.EFCore.PostgreSQL` | `postgresql` |
| `Genocs.Persistence.EFCore.MySql` | `mysql` (not yet supported on net10.0) |
| `Genocs.Persistence.EFCore.Sqlite` | `sqlite` |
| `Genocs.Persistence.EFCore.Oracle` | `oracle` |
| `Genocs.Persistence.EFCore.MongoDB` | `mongodb` (no migrations; seeding only) |

For Finbuckle-based multitenancy with a SQL Server tenant store, add `Genocs.Persistence.EFCore.MultiTenancy.SqlServer`.

## Installation

```bash
dotnet add package Genocs.Persistence.EFCore
dotnet add package Genocs.Persistence.EFCore.SqlServer
```

## Getting Started

Register the provider, then wire EF Core persistence. Provider selection is config-driven via the `DatabaseOptions` section:

```csharp
builder.Services.AddSqlServerDbProvider();

builder.AddGenocs()
    .AddEFCorePersistence();
```

```json
{
  "DatabaseOptions": {
    "DBProvider": "mssql",
    "ConnectionString": "Server=localhost;Database=app;Integrated Security=True;",
    "AutoApplyMigrations": false
  }
}
```

Relational providers expect migrations in a `Migrators.*` assembly (e.g. `Migrators.MSSQL`). With `AutoApplyMigrations` set to `false` (the default), pending migrations stop the host at startup instead of being applied.

## Main Entry Points

- `AddEFCorePersistence`
- `IEFCoreDbProvider` (implemented by the provider packages)
- `IDatabaseInitializer`
- `ICustomSeeder`

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
