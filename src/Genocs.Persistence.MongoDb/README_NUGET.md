# Genocs.Persistence.MongoDB

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

MongoDB repository and persistence integration for Genocs applications. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Persistence.MongoDB
```

## Getting Started

Use this package to configure MongoDB repositories and persistence registrations for Genocs services.

Service registration:

```csharp
using Genocs.Persistence.MongoDB.Extensions;

genocs.AddMongoWithRegistration();
```

Configuration example:

```json
{
  "mongoDb": {
    "connectionString": "mongodb://localhost:27017",
    "database": "genocs_db"
  }
}
```

## Main Entry Points

- `AddMongo`
- `AddMongoWithRegistration`

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
