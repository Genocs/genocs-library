# Genocs.Saga.Integrations.MongoDB

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

MongoDB storage integration for Genocs saga state management. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Saga.Integrations.MongoDB
```

## Getting Started

Use this package to persist saga state and correlation data in MongoDB when used with `Genocs.Saga`.

MongoDB client management is delegated to `Genocs.Persistence.MongoDB`, so the saga
integration shares the same `IMongoClient` and `IMongoDatabase` registrations used by
the rest of the Genocs host. This avoids duplicate client instances and centralizes
MongoDB configuration in a single place.

```csharp
// Configure MongoDB once via Genocs.Persistence.MongoDB
genocs.AddMongo();

builder.Services.AddSaga(saga =>
{
    // Reuse the IMongoDatabase registered above.
    saga.UseMongoPersistence();
});
```

Alternative overloads still let you bind options from configuration or supply
`MongoOptions` explicitly. In both cases the MongoDB client is registered through
`Genocs.Persistence.MongoDB` so a single client pipeline is reused.

```csharp
saga.UseMongoPersistence(builder.Configuration);            // reads "mongoDb" section
saga.UseMongoPersistence(builder.Configuration, "myMongo"); // custom section name
saga.UseMongoPersistence(new MongoOptions { ... });          // explicit options
```

## Main Entry Points

- `UseMongoPersistence` (parameterless, with `IConfiguration`, or with `MongoOptions`)

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
