# Genocs.Persistence.EFCore.MySql

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

MySQL database provider for Genocs EF Core persistence. Supports `net10.0`, `net9.0`, and `net8.0`.

Note: on `net10.0` the provider is a stub that throws `NotSupportedException` at configuration time, until Pomelo.EntityFrameworkCore.MySql supports EF Core 10.

## Installation

```bash
dotnet add package Genocs.Persistence.EFCore.MySql
```

## Getting Started

Register the provider alongside `AddEFCorePersistence` and set `DatabaseOptions:DBProvider` to `mysql`.

## Main Entry Points

- `AddMySqlDbProvider`

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
