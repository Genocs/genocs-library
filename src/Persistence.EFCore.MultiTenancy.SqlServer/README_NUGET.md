# Genocs.Persistence.EFCore.MultiTenancy.SqlServer

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Finbuckle-based multitenancy for Genocs EF Core persistence, with a SQL Server-backed tenant store. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Persistence.EFCore.MultiTenancy.SqlServer
```

## Getting Started

Register the EF Core tenant store alongside `AddEFCorePersistence`. The tenant store database (`TenantDbContext`) always uses SQL Server.

## Main Entry Points

- `AddFinbuckleMultiTenancy`
- `AddFinbuckleMultiTenancyWithEfCoreStore`
- `UseMultiTenancy`

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
