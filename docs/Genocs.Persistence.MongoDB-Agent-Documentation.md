# Genocs.Persistence.MongoDB Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

Genocs.Persistence.MongoDB provides MongoDB connectivity and repository wiring for Genocs services. It registers `IMongoClient`, `IMongoDatabase`, a generic `IMongoRepository<TEntity>`, session factory support, and optional collection seeding — all from a single `mongoDb` configuration section.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Persistence.MongoDB` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | MongoDB persistence and repository integration |
| Typical startup APIs | `AddMongo`, `AddMongoWithRegistration` |

## Install

```bash
dotnet add package Genocs.Persistence.MongoDB
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();

gnxBuilder
    .AddMongoWithRegistration()
    .Build();

var app = builder.Build();
app.Run();
```

## Configuration

Use the `mongoDb` section in `appsettings.json`.

```json
{
    "mongoDb": {
        "connectionString": "mongodb://localhost:27017",
        "database": "genocs_orders",
        "enableTracing": true,
        "seed": false,
        "setRandomDatabaseSuffix": false
    }
}
```

| Setting | Type | Description |
|---|---|---|
| `connectionString` | `string` | MongoDB connection string. Required. |
| `database` | `string` | Target database name. Required. |
| `enableTracing` | `bool` | Subscribes the Mongo diagnostic activity source for tracing. |
| `seed` | `bool` | Triggers the registered `IMongoSeeder` during initialization. |
| `setRandomDatabaseSuffix` | `bool` | Appends a random GUID suffix to the database name, mainly for tests. |

The option class includes validation semantics through `MongoOptions.IsValid(...)`: registration is skipped when `connectionString` or `database` is empty.

## Decision Matrix For Agents

| Goal | Preferred API | Why |
|---|---|---|
| Register Mongo connectivity only | `AddMongo()` | Minimal setup when repositories are custom-registered |
| Register Mongo plus a scoped generic repository | `AddMongoWithRegistration()` | Fastest path; registers `IMongoRepository<T>` for all entities automatically |
| Register a specific typed repository | `AddMongoRepository<TEntity, TKey>(collectionName)` | Pins a collection name for entities with non-ObjectId keys |
| Scan and register custom repository implementations | `RegisterMongoRepositories(assembly)` | Convention-based scanning of all `IMongoRepository<T>` implementations |
| Access `IMongoDatabase` or `IMongoClient` directly | `IMongoDatabaseProvider` (resolved from DI) | Single abstraction for low-level driver access |
| Create a driver session for transactions | `IMongoSessionFactory.CreateAsync()` | Returns an `IClientSessionHandle` for multi-document transactions |

## Behavior Notes / Constraints

- `AddMongo` skips registration silently when the `mongoDb` section is absent or the connection string and database name are empty.
- BSON conventions (camelCase field names, string enums, `Decimal128`, `CSharpLegacy` GUIDs) are applied globally once per process.
- `AddMongoWithRegistration` registers `IMongoRepository<T>` with scoped lifetime; prefer scoped consumers.
- MongoDB connectivity is not verified at startup; connection errors surface on the first operation.
- Seeding runs through `IMongoSeeder` at initialization time only when `seed: true` is set.

## Public Capability Map

| Capability | Surface |
|---|---|
| Register Mongo connectivity from configuration | `AddMongo` on `IGenocsBuilder` |
| Register Mongo plus scoped generic repository | `AddMongoWithRegistration` on `IGenocsBuilder` |
| Register a named collection-bound repository | `AddMongoRepository<TEntity, TKey>` on `IGenocsBuilder` |
| Scan assembly for repository implementations | `RegisterMongoRepositories(assembly)` on `IGenocsBuilder` |
| Typed entity repository contract | `IMongoRepository<TEntity>` (ObjectId key) |
| Typed repository with custom key | `IMongoBaseRepository<TEntity, TKey>` |
| Direct database and client access | `IMongoDatabaseProvider` |
| Create driver sessions for transactions | `IMongoSessionFactory` |

## Dependencies

- `Genocs.Core`
- `MongoDB.Driver`
- `MongoDB.Driver.Core.Extensions.DiagnosticSources`

## Troubleshooting

1. Repository services cannot be resolved from DI.
Fix: Use `AddMongoWithRegistration()` to register the generic `IMongoRepository<T>`, or call `AddMongoRepository<TEntity, TKey>()` for typed collection mappings explicitly before `Build()`.
2. Application fails to connect to MongoDB outside local development.
Fix: Validate `mongoDb.connectionString`, ensure credentials and TLS settings are correct, and confirm that network and firewall rules permit the connection.
3. Seed data does not execute during startup.
Fix: Set `seed: true` in the `mongoDb` configuration section and ensure a custom `IMongoSeeder` implementation is registered, or that the default seeder initialization flow is not suppressed.
