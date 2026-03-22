# Genocs.Saga.Integrations.Redis Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request package version and configuration details.

## Purpose

`Genocs.Saga.Integrations.Redis` replaces default in-memory saga persistence with Redis-backed saga state and saga log storage for distributed deployments.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Saga.Integrations.Redis` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Redis persistence provider for saga state and logs |
| Main APIs | `UseRedisPersistence(ISagaBuilder, SagaRedisOptions)`, `UseRedisPersistence(ISagaBuilder, string, IConfiguration)` |

## Install

```bash
dotnet add package Genocs.Saga.Integrations.Redis
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Saga;
using Genocs.Saga.Integrations.Redis;
using Genocs.Saga.Integrations.Redis.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSaga(saga =>
{
    saga.UseRedisPersistence(new SagaRedisOptions
    {
        Configuration = "localhost:6379",
        InstanceName = "genocs-saga"
    });
});

var app = builder.Build();
app.Run();
```

## Configuration

Use `SagaRedisOptions` directly or provide a configuration section value that deserializes into that type.

```json
{
    "sagaRedis": {
        "enabled": true,
        "configuration": "localhost:6379",
        "instanceName": "genocs-saga"
    }
}
```

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Option flag available in the model. Registration still depends on calling `UseRedisPersistence(...)`. |
| `configuration` | `string` | Redis connection string passed to `AddStackExchangeRedisCache`. |
| `instanceName` | `string` | Cache key prefix used for saga state and log entries. |

The current options type is `SagaRedisOptions` and its default section name is `sagaRedis`.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Configure Redis persistence with explicit values | `UseRedisPersistence(ISagaBuilder, SagaRedisOptions)` |
| Configure Redis persistence from app configuration | `UseRedisPersistence(ISagaBuilder, string, IConfiguration)` |
| Replace in-memory saga persistence | Call `UseRedisPersistence(...)` inside `AddSaga(...)` |
| Keep saga action contracts unchanged | Use Redis integration only at registration time |

## Behavior Notes / Constraints

- Must be configured inside `AddSaga(...)` registration.
- Configuration overload expects a section value format that can be deserialized into `SagaRedisOptions`.
- Persisted payload compatibility affects rehydration after contract changes.

## Public Capability Map

- Builder extensions for Redis persistence registration.
- Redis-backed `ISagaStateRepository` implementation.
- Redis-backed `ISagaLog` implementation.

## Dependencies

- `Genocs.Saga`
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `Microsoft.Extensions.Configuration`
- `Newtonsoft.Json`

## Troubleshooting

1. Saga data is not shared across service instances.
Fix: Verify Redis connectivity and ensure `UseRedisPersistence(...)` is called in startup.
2. Startup throws during Redis settings binding.
Fix: Validate the configuration section value format and required fields (`configuration`, `instanceName`).
3. Saga state or log payloads fail to rehydrate after deployment.
Fix: Keep serialized saga contracts backward-compatible when evolving message and state types.
