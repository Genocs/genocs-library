# Genocs.Saga.Integrations.Redis Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Saga.Integrations.Redis is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Saga.Integrations.Redis |
| Project file | [src/Genocs.Saga.Integrations.Redis/Genocs.Saga.Integrations.Redis.csproj](src/Genocs.Saga.Integrations.Redis/Genocs.Saga.Integrations.Redis.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | Redis-backed saga state and log persistence integration for Genocs.Saga |
| Core themes | UseRedisPersistence extensions, StackExchangeRedis cache registration, JSON serialization of saga state/log, Redis key-based persistence |

## Use This Package When

- Persisting saga state and saga log entries to Redis.
- Sharing saga persistence across service instances with a distributed cache backend.
- Registering saga persistence with StackExchange Redis cache options.
- Running compensation flows that depend on persisted saga log history.
- Using explicit SagaRedisSettings in startup wiring.

## Do Not Assume

- UseRedisPersistence with IConfiguration expects the section value to be JSON text, not a standard object-bound section.
- Redis persistence uses key patterns based on id and type hash code, not typed collections.
- Write paths assume non-null message and state data values when calling GetType.

## High-Value Entry Points

### Integration registration

- UseRedisPersistence(ISagaBuilder, string, IConfiguration) in [src/Genocs.Saga.Integrations.Redis/Extensions.cs](src/Genocs.Saga.Integrations.Redis/Extensions.cs)
- UseRedisPersistence(ISagaBuilder, SagaRedisSettings) in [src/Genocs.Saga.Integrations.Redis/Extensions.cs](src/Genocs.Saga.Integrations.Redis/Extensions.cs)

### Configuration model

- SagaRedisSettings in [src/Genocs.Saga.Integrations.Redis/ChroncileRedisSettings.cs](src/Genocs.Saga.Integrations.Redis/ChroncileRedisSettings.cs)

### Redis persistence implementations

- RedisSagaStateRepository in [src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaStateRepository.cs](src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaStateRepository.cs)
- RedisSagaLog in [src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaLog.cs](src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaLog.cs)
- RedisSagaState in [src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaState.cs](src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaState.cs)
- RedisSagaLogData in [src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaLogData.cs](src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaLogData.cs)

### Core saga contracts consumed

- ISagaBuilder in [src/Genocs.Saga/ISagaBuilder.cs](src/Genocs.Saga/ISagaBuilder.cs)
- ISagaStateRepository in [src/Genocs.Saga/ISagaStateRepository.cs](src/Genocs.Saga/ISagaStateRepository.cs)
- ISagaLog in [src/Genocs.Saga/ISagaLog.cs](src/Genocs.Saga/ISagaLog.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Enable Redis persistence from explicit settings | UseRedisPersistence(builder, SagaRedisSettings) | Most direct and predictable registration path |
| Enable Redis persistence from configuration | UseRedisPersistence(builder, sectionName, IConfiguration) | Expects JSON string in section value and deserializes with Newtonsoft.Json |
| Persist saga state snapshot | ISagaStateRepository.WriteAsync via RedisSagaStateRepository | Stores serialized RedisSagaState under generated state key |
| Reload saga state for next message | ISagaStateRepository.ReadAsync via RedisSagaStateRepository | Deserializes state and rehydrates Data from JObject with DataType |
| Persist saga log history | ISagaLog.WriteAsync via RedisSagaLog | Reads current list, appends new log item, writes back serialized list |
| Load compensation log history | ISagaLog.ReadAsync via RedisSagaLog | Returns deserialized log entries for saga id and type |
| Delete saga state manually | RedisSagaStateRepository.DeleteAsync | Removes state key from distributed cache |
| Delete saga log manually | RedisSagaLog.DeleteAsync | Removes log key from distributed cache |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Saga.Integrations.Redis
```

### Setup in Program.cs

```csharp
using Genocs.Saga;
using Genocs.Saga.Integrations.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSaga(saga =>
{
    saga.UseRedisPersistence(new SagaRedisSettings
    {
        Configuration = "localhost:6379",
        InstanceName = "genocs-saga"
    });
});

var app = builder.Build();

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- Configuration overload deserializes from configuration section raw value; malformed or missing JSON triggers SagaException.
- ConfigureRedisPersistence registers IDistributedCache via AddStackExchangeRedisCache.
- State keys and log keys are computed using id plus type hash code suffix.
- RedisSagaStateRepository rehydrates Data from JObject using stored DataType after deserialization.
- Read and write methods validate required arguments and throw SagaException on null/whitespace inputs.
- Log writes use read-modify-write of the full log list under one key.

## Source-Accurate Capability Map

### Builder integration and DI wiring

- Extends ISagaBuilder to plug Redis persistence into saga runtime.
- Supports settings from explicit object or configuration value deserialization.
- Registers distributed cache and binds saga log/state contracts to Redis implementations.

Files:

- [src/Genocs.Saga.Integrations.Redis/Extensions.cs](src/Genocs.Saga.Integrations.Redis/Extensions.cs)
- [src/Genocs.Saga.Integrations.Redis/ChroncileRedisSettings.cs](src/Genocs.Saga.Integrations.Redis/ChroncileRedisSettings.cs)

### Saga state persistence on distributed cache

- Stores and retrieves serialized RedisSagaState payloads per saga key.
- Rehydrates dynamic saga data payload to original runtime type when DataType is available.
- Supports explicit state deletion.

Files:

- [src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaStateRepository.cs](src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaStateRepository.cs)
- [src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaState.cs](src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaState.cs)

### Saga log persistence on distributed cache

- Stores saga log entry list as serialized JSON in cache.
- Rehydrates logged message payloads by stored message type.
- Supports explicit log deletion.

Files:

- [src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaLog.cs](src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaLog.cs)
- [src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaLogData.cs](src/Genocs.Saga.Integrations.Redis/Persistence/RedisSagaLogData.cs)

### Core saga contract interoperability

- Integrates through ISagaBuilder extension points.
- Implements ISagaStateRepository and ISagaLog interfaces expected by saga managers.

Files:

- [src/Genocs.Saga/ISagaBuilder.cs](src/Genocs.Saga/ISagaBuilder.cs)
- [src/Genocs.Saga/ISagaStateRepository.cs](src/Genocs.Saga/ISagaStateRepository.cs)
- [src/Genocs.Saga/ISagaLog.cs](src/Genocs.Saga/ISagaLog.cs)

## Dependencies

From [src/Genocs.Saga.Integrations.Redis/Genocs.Saga.Integrations.Redis.csproj](src/Genocs.Saga.Integrations.Redis/Genocs.Saga.Integrations.Redis.csproj):

- Genocs.Saga (project reference in Debug, package reference in Release)
- Microsoft.Extensions.Caching.StackExchangeRedis
- Newtonsoft.Json
- Microsoft.Extensions.Configuration

## Related Docs

- NuGet package readme: [src/Genocs.Saga.Integrations.Redis/README_NUGET.md](src/Genocs.Saga.Integrations.Redis/README_NUGET.md)
- Saga package doc: [docs/Genocs.Saga-Agent-Documentation.md](docs/Genocs.Saga-Agent-Documentation.md)
- Repository guide: [README.md](README.md)
