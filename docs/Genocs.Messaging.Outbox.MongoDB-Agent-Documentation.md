# Genocs.Messaging.Outbox.MongoDB Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

`Genocs.Messaging.Outbox.MongoDB` provides MongoDB-backed durable storage for inbox/outbox records used by the base outbox runtime.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Messaging.Outbox.MongoDB` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | MongoDB durable outbox and inbox persistence |
| Core entry points | `AddMessageOutbox(o => o.AddMongo())`, `IMessageOutbox`, `IMessageOutboxAccessor` |

## Install

```bash
dotnet add package Genocs.Messaging.Outbox.MongoDB
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.Outbox;
using Genocs.Messaging.Outbox.MongoDB;
using Genocs.Persistence.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();
gnxBuilder
    .AddMongo()
    .AddMessageOutbox(o => o.AddMongo());

gnxBuilder.Build();

var app = builder.Build();
app.Run();
```

## Configuration

This integration uses two sections together:

1. `outbox` from `Genocs.Messaging.Outbox`
2. `mongoDb` from `Genocs.Persistence.MongoDB`

```json
{
    "outbox": {
        "enabled": true,
        "intervalMilliseconds": 5000,
        "expiry": 3600,
        "inboxCollection": "inbox",
        "outboxCollection": "outbox",
        "type": "sequential",
        "disableTransactions": false
    },
    "mongoDb": {
        "connectionString": "mongodb://localhost:27017",
        "database": "genocs_outbox",
        "enableTracing": true,
        "seed": false,
        "setRandomDatabaseSuffix": false
    }
}
```

| Section | Setting | Type | Description |
|---|---|---|---|
| `outbox` | `enabled` | `bool` | Enables the base outbox runtime. |
| `outbox` | `intervalMilliseconds` | `double` | Poll interval used to fetch unsent records. |
| `outbox` | `expiry` | `int` | Enables cleanup of processed inbox/outbox records when positive. |
| `outbox` | `inboxCollection` | `string` | Mongo inbox collection name. |
| `outbox` | `outboxCollection` | `string` | Mongo outbox collection name. |
| `outbox` | `type` | `string` | Processing strategy used by the base outbox runtime. |
| `outbox` | `disableTransactions` | `bool` | Disables Mongo session/transaction usage for handler and persistence flows. |
| `mongoDb` | `connectionString` | `string` | MongoDB connection string used by the provider. |
| `mongoDb` | `database` | `string` | Target database name. |
| `mongoDb` | `enableTracing` | `bool` | Enables Mongo driver tracing. |
| `mongoDb` | `seed` | `bool` | Seeds Mongo storage when initialization flow uses a seeder. |
| `mongoDb` | `setRandomDatabaseSuffix` | `bool` | Appends a random suffix to the database name, mainly for isolated test runs. |

The provider does not introduce a new section of its own. It composes the base `outbox` settings with the existing MongoDB connectivity settings.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Keep outbox API and switch persistence to MongoDB | `AddMessageOutbox(o => o.AddMongo())` |
| Customize inbox and outbox collection names | `outbox.inboxCollection` and `outbox.outboxCollection` |
| Enable record cleanup by expiry | `outbox.expiry` |
| Disable Mongo transaction usage for constrained deployments | `outbox.disableTransactions` |
| Load and mark pending records | `IMessageOutboxAccessor.GetUnsentAsync()` and `ProcessAsync(...)` |

## Behavior Notes / Constraints

- This package extends base outbox registration and does not replace it.
- Persisted payloads are serialized and later rehydrated, so runtime message types must remain resolvable.
- Transaction behavior depends on MongoDB session support and `outbox.disableTransactions`.

## Public Capability Map

- Mongo outbox provider extension through `AddMongo` on outbox configurator.
- Mongo-backed implementations of `IMessageOutbox` and `IMessageOutboxAccessor`.
- Expiry-driven processed-record cleanup support through Mongo TTL behavior.

## Dependencies

- `Genocs.Messaging.Outbox`
- `Genocs.Persistence.MongoDB`

## Troubleshooting

1. Pending records fail while rehydrating message payloads.
Fix: Keep message contracts version-compatible and ensure required runtime types are loadable.
2. Processed records never expire.
Fix: Set a positive `outbox.expiry` and confirm index creation permissions in MongoDB.
3. Inbox handling fails with Mongo session or transaction errors.
Fix: Set `outbox.disableTransactions` to true when deployment topology does not support required transaction features.
