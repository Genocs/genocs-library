# Genocs.Persistence.EFCore.MongoDB

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

MongoDB database provider for Genocs EF Core persistence. Supports `net10.0`, `net9.0`, and `net8.0`.

Note: the MongoDB EF Core provider does not support migrations; database initialization only runs seeders.

## Installation

```bash
dotnet add package Genocs.Persistence.EFCore.MongoDB
```

## Getting Started

Register the provider alongside `AddEFCorePersistence` and set `DatabaseOptions:DBProvider` to `mongodb`.

## Main Entry Points

- `AddMongoDbProvider`

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
