# Genocs.Saga.Integrations.MongoDB Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

Genocs.Saga.Integrations.MongoDB replaces the default in-memory saga persistence in `Genocs.Saga` with durable MongoDB-backed storage for both saga state and saga logs. It exposes two `UseMongoPersistence` extension methods on `ISagaBuilder` — one that reads from `IConfiguration` and one that accepts an explicit `SagaMongoOptions` object — both registering `MongoSagaStateRepository` and `MongoSagaLog` implementations transparently.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Saga.Integrations.MongoDB` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | MongoDB-backed saga state and log persistence |
| Typical startup APIs | `AddSaga` + `UseMongoPersistence` |

## Install

```bash
dotnet add package Genocs.Saga.Integrations.MongoDB
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Saga;
using Genocs.Saga.Integrations.MongoDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSaga(saga =>
{
    saga.UseMongoPersistence(builder.Configuration);
});

var app = builder.Build();
app.Run();
```

## Configuration

Use the `sagaMongo` section in `appsettings.json`.

```json
{
    "sagaMongo": {
        "enabled": true,
        "connectionString": "mongodb://localhost:27017",
        "database": "genocs_saga"
    }
}
```

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Option flag available in the model. Registration still depends on calling `UseMongoPersistence(...)`. |
| `connectionString` | `string` | MongoDB connection string for saga persistence. Required. |
| `database` | `string` | Database name for saga state and log collections. Required. |

The section name changed to `sagaMongo` with `SagaMongoOptions.Position`. Use that name in new hosts and updated documentation.

## Decision Matrix For Agents

| Goal | Preferred API | Why |
|---|---|---|
| Configure persistence from application config | `saga.UseMongoPersistence(configuration)` | Reads `sagaMongo` section from host `IConfiguration` |
| Configure persistence from explicit options | `saga.UseMongoPersistence(new SagaMongoOptions { ... })` | Useful in tests and scenarios with programmatic configuration |
| Replace default in-memory persistence | `AddSaga(s => s.UseMongoPersistence(…))` | Keeps saga action contracts unchanged; only the persistence layer is replaced |
| Ensure state and logs survive restarts | Mongo-backed `ISagaStateRepository` and `ISagaLog` | Documents survive process restarts and multi-instance deployment |

## Behavior Notes / Constraints

- `UseMongoPersistence` must be called inside the `AddSaga` builder callback; calling it after `AddSaga` returns has no effect.
- `connectionString` and `database` are both required; missing or empty values throw `SagaException` at startup.
- Payload and state objects are serialized to BSON; breaking changes to saga message or state contracts can make stored documents unreadable.
- MongoDB permissions must include read and write access on the target database and the saga collections.

## Public Capability Map

| Capability | Surface |
|---|---|
| Configure Mongo persistence from host configuration | `UseMongoPersistence(ISagaBuilder, IConfiguration)` |
| Configure Mongo persistence from explicit settings | `UseMongoPersistence(ISagaBuilder, SagaMongoOptions)` |
| Durable saga state storage | `ISagaStateRepository` backed by `MongoSagaStateRepository` |
| Durable saga log storage | `ISagaLog` backed by `MongoSagaLog` |

## Dependencies

- `Genocs.Saga`
- `MongoDB.Driver`
- `Microsoft.Extensions.Configuration`

## Troubleshooting

1. Saga still behaves as if using in-memory persistence after adding this package.
Fix: Ensure `UseMongoPersistence` is called inside the `AddSaga(saga => { ... })` builder callback during startup; calling it after `AddSaga` returns has no effect.
2. Startup throws `SagaException` while loading saga Mongo settings.
Fix: Validate that `sagaMongo.connectionString` and `sagaMongo.database` are present and non-empty in `appsettings.json`, and confirm network connectivity and credentials to the MongoDB instance.
3. Compensation history is missing or unreadable after a deployment update.
Fix: Maintain backward compatibility in serialized saga message and state contracts across versions; avoid renaming properties or changing types stored in existing saga log documents.
