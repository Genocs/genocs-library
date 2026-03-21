# Genocs.Saga.Integrations.MongoDB Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Saga.Integrations.MongoDB is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Saga.Integrations.MongoDB |
| Project file | [src/Genocs.Saga.Integrations.MongoDB/Genocs.Saga.Integrations.MongoDB.csproj](src/Genocs.Saga.Integrations.MongoDB/Genocs.Saga.Integrations.MongoDB.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | MongoDB-backed saga state and log persistence implementation for Genocs.Saga |
| Core themes | UseMongoPersistence extensions, SagaMongoOption config, MongoSagaStateRepository, MongoSagaLog |

## Use This Package When

- Persisting saga state and saga logs into MongoDB collections.
- Replacing default in-memory saga persistence with durable storage.
- Configuring saga Mongo persistence from app configuration.
- Passing explicit Mongo settings in tests or controlled environments.
- Sharing saga persistence across multiple application instances.

## Do Not Assume

- UseMongoPersistence requires Genocs.Saga AddSaga registration; this package does not register saga coordinator pipeline by itself.
- Configuration overload throws SagaException when settings deserialization/access fails.
- Mongo persistence uses fixed collection names SagaData and SagaLog.

## High-Value Entry Points

### Integration registration

- UseMongoPersistence(ISagaBuilder, IConfiguration) in [src/Genocs.Saga.Integrations.MongoDB/Extensions.cs](src/Genocs.Saga.Integrations.MongoDB/Extensions.cs)
- UseMongoPersistence(ISagaBuilder, SagaMongoOption) in [src/Genocs.Saga.Integrations.MongoDB/Extensions.cs](src/Genocs.Saga.Integrations.MongoDB/Extensions.cs)

### Configuration model

- SagaMongoOption in [src/Genocs.Saga.Integrations.MongoDB/SagaMongoOption.cs](src/Genocs.Saga.Integrations.MongoDB/SagaMongoOption.cs)

### Mongo persistence implementations

- MongoSagaStateRepository in [src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaStateRepository.cs](src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaStateRepository.cs)
- MongoSagaLog in [src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaLog.cs](src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaLog.cs)
- MongoSagaState in [src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaState.cs](src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaState.cs)
- MongoSagaLogData in [src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaLogData.cs](src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaLogData.cs)

### Core saga abstractions consumed

- ISagaBuilder in [src/Genocs.Saga/ISagaBuilder.cs](src/Genocs.Saga/ISagaBuilder.cs)
- ISagaStateRepository in [src/Genocs.Saga/ISagaStateRepository.cs](src/Genocs.Saga/ISagaStateRepository.cs)
- ISagaLog in [src/Genocs.Saga/ISagaLog.cs](src/Genocs.Saga/ISagaLog.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Enable Mongo persistence via configuration | UseMongoPersistence(builder, IConfiguration) | Reads SagaMongo section using SagaMongoOption.Position |
| Enable Mongo persistence with explicit settings | UseMongoPersistence(builder, SagaMongoOption) | Best for tests and explicit startup wiring |
| Persist saga state snapshot | ISagaStateRepository.WriteAsync via MongoSagaStateRepository | Replaces existing document by saga ID and type then inserts new state |
| Load saga state for rehydration | ISagaStateRepository.ReadAsync via MongoSagaStateRepository | Returns first state doc matching saga ID and saga type |
| Persist saga log entry | ISagaLog.WriteAsync via MongoSagaLog | Appends message entry to SagaLog collection |
| Read compensation history | ISagaLog.ReadAsync via MongoSagaLog | Reads all log rows for saga ID and type |
| Set custom Mongo database connection | SagaMongoOption.ConnectionString and Database | Passed to MongoClient and GetDatabase |
| Replace storage provider without changing saga core | ISagaBuilder.UseSagaLog and UseSagaStateRepository calls in extension | Package binds both interfaces to Mongo implementations |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Saga.Integrations.MongoDB
```

### Setup in Program.cs

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

### Configuration

```json
{
  "SagaMongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "genocs_saga"
  }
}
```

## Behavior Notes That Affect Agent Decisions

- Configuration-based registration catches failures and throws SagaException with a generic deserialization error message.
- The integration registers IMongoDatabase through a transient factory delegate.
- State writes are implemented as delete-then-insert, not atomic update.
- Saga type identity is persisted as Type.FullName string and used for filtering reads.
- MongoSagaState resolves runtime Type by scanning loaded AppDomain assemblies.
- MongoSagaLogData resolves ISagaLogData.Type from Assembly.GetEntryAssembly(), which can be null in some host/test scenarios.

## Source-Accurate Capability Map

### Builder integration and wiring

- Extends ISagaBuilder with Mongo persistence overloads.
- Supports both IConfiguration-driven and explicit option-driven setup.
- Registers Mongo-backed implementations for saga log and state repository.

Files:

- [src/Genocs.Saga.Integrations.MongoDB/Extensions.cs](src/Genocs.Saga.Integrations.MongoDB/Extensions.cs)
- [src/Genocs.Saga.Integrations.MongoDB/SagaMongoOption.cs](src/Genocs.Saga.Integrations.MongoDB/SagaMongoOption.cs)

### Saga state persistence in MongoDB

- Stores state records in SagaData collection.
- Reads by saga ID and saga type full name.
- Persists state object payload and saga state enum values.

Files:

- [src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaStateRepository.cs](src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaStateRepository.cs)
- [src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaState.cs](src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaState.cs)

### Saga log persistence in MongoDB

- Stores message logs in SagaLog collection.
- Reads log stream for specific saga ID and saga type.
- Persists created-at timestamp and original message payload.

Files:

- [src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaLog.cs](src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaLog.cs)
- [src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaLogData.cs](src/Genocs.Saga.Integrations.MongoDB/Persistence/MongoSagaLogData.cs)

### Core saga contract interoperability

- Integrates through ISagaBuilder customization hooks.
- Implements ISagaStateRepository and ISagaLog contracts expected by Genocs.Saga managers.

Files:

- [src/Genocs.Saga/ISagaBuilder.cs](src/Genocs.Saga/ISagaBuilder.cs)
- [src/Genocs.Saga/ISagaStateRepository.cs](src/Genocs.Saga/ISagaStateRepository.cs)
- [src/Genocs.Saga/ISagaLog.cs](src/Genocs.Saga/ISagaLog.cs)

## Dependencies

From [src/Genocs.Saga.Integrations.MongoDB/Genocs.Saga.Integrations.MongoDB.csproj](src/Genocs.Saga.Integrations.MongoDB/Genocs.Saga.Integrations.MongoDB.csproj):

- Genocs.Core (project reference in Debug, package reference in Release)
- Genocs.Saga (project reference in Debug, package reference in Release)
- Microsoft.Extensions.Configuration
- MongoDB.Driver

## Related Docs

- NuGet package readme: [src/Genocs.Saga.Integrations.MongoDB/README_NUGET.md](src/Genocs.Saga.Integrations.MongoDB/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.Saga.Integrations.MongoDB-Agent-Documentation.md](docs/Genocs.Saga.Integrations.MongoDB-Agent-Documentation.md)
- Saga package doc: [docs/Genocs.Saga-Agent-Documentation.md](docs/Genocs.Saga-Agent-Documentation.md)
