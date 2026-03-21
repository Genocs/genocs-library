# Genocs.Saga Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Saga is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Saga |
| Project file | [src/Genocs.Saga/Genocs.Saga.csproj](src/Genocs.Saga/Genocs.Saga.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | Saga orchestration abstractions and execution pipeline for distributed workflows |
| Core themes | AddSaga registration, pluggable persistence, saga discovery, coordinated execution, compensation handling |

## Use This Package When

- Implementing long-running workflow orchestration with correlation IDs.
- Managing saga state transitions from pending to completed/rejected.
- Running compensation logic for rejected saga flows.
- Using default in-memory saga state/log persistence.
- Integrating custom saga persistence through ISagaBuilder.

## Do Not Assume

- AddSaga defaults to in-memory persistence when no build action is provided.
- Saga instances are discovered from currently loaded AppDomain assemblies only.
- Rejected saga instances are not reinitialized by SagaInitializer.

## High-Value Entry Points

### Startup and registration

- AddSaga in [src/Genocs.Saga/Extensions.cs](src/Genocs.Saga/Extensions.cs)
- ISagaBuilder in [src/Genocs.Saga/ISagaBuilder.cs](src/Genocs.Saga/ISagaBuilder.cs)
- SagaBuilder in [src/Genocs.Saga/Builders/SagaBuilder.cs](src/Genocs.Saga/Builders/SagaBuilder.cs)

### Runtime orchestration pipeline

- ISagaCoordinator in [src/Genocs.Saga/ISagaCoordinator.cs](src/Genocs.Saga/ISagaCoordinator.cs)
- SagaCoordinator in [src/Genocs.Saga/Managers/SagaCoordinator.cs](src/Genocs.Saga/Managers/SagaCoordinator.cs)
- SagaInitializer in [src/Genocs.Saga/Managers/SagaInitializer.cs](src/Genocs.Saga/Managers/SagaInitializer.cs)
- SagaProcessor in [src/Genocs.Saga/Managers/SagaProcessor.cs](src/Genocs.Saga/Managers/SagaProcessor.cs)
- SagaPostProcessor in [src/Genocs.Saga/Managers/SagaPostProcessor.cs](src/Genocs.Saga/Managers/SagaPostProcessor.cs)
- SagaSeeker in [src/Genocs.Saga/Managers/SagaSeeker.cs](src/Genocs.Saga/Managers/SagaSeeker.cs)

### Saga contracts and lifecycle types

- ISaga and ISaga<TData> in [src/Genocs.Saga/ISaga.cs](src/Genocs.Saga/ISaga.cs)
- Saga and Saga<TData> in [src/Genocs.Saga/Saga.cs](src/Genocs.Saga/Saga.cs)
- ISagaAction and ISagaStartAction in [src/Genocs.Saga/ISagaAction.cs](src/Genocs.Saga/ISagaAction.cs)
- ISagaStateRepository in [src/Genocs.Saga/ISagaStateRepository.cs](src/Genocs.Saga/ISagaStateRepository.cs)
- ISagaLog in [src/Genocs.Saga/ISagaLog.cs](src/Genocs.Saga/ISagaLog.cs)

### Default in-memory persistence

- InMemorySagaStateRepository in [src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs](src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs)
- InMemorySagaLog in [src/Genocs.Saga/Persistence/InMemorySagaLog.cs](src/Genocs.Saga/Persistence/InMemorySagaLog.cs)
- SagaState in [src/Genocs.Saga/Persistence/SagaState.cs](src/Genocs.Saga/Persistence/SagaState.cs)
- SagaLogData in [src/Genocs.Saga/Persistence/SagaLogData.cs](src/Genocs.Saga/Persistence/SagaLogData.cs)

### Context and concurrency helpers

- SagaContext in [src/Genocs.Saga/SagaContext.cs](src/Genocs.Saga/SagaContext.cs)
- SagaContextBuilder in [src/Genocs.Saga/Builders/SagaContextBuilder.cs](src/Genocs.Saga/Builders/SagaContextBuilder.cs)
- KeyedLocker in [src/Genocs.Saga/Async/KeyedLocker.cs](src/Genocs.Saga/Async/KeyedLocker.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Register saga orchestration with defaults | IServiceCollection.AddSaga() | Uses in-memory log and state repository automatically |
| Register saga orchestration with custom persistence | AddSaga(build => ...) | Configure ISagaBuilder persistence before registration completes |
| Process incoming message through all matching saga actions | ISagaCoordinator.ProcessAsync<TMessage>(...) | Runs discovery, initialization, handling, and post-processing |
| Resolve and initialize saga instance from message | SagaInitializer.TryInitializeAsync | Rejects non-start actions when no state exists |
| Execute message handler and persist state/log | SagaProcessor.ProcessAsync | Persists state and log in finally block |
| Trigger completion/rejection hooks and compensation | SagaPostProcessor.ProcessAsync | Runs compensation in reverse log order for rejected state |
| Register custom log provider | ISagaBuilder.UseSagaLog<TSagaLog>() | Replace default in-memory log implementation |
| Register custom state repository | ISagaBuilder.UseSagaStateRepository<TRepository>() | Replace default in-memory state store |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Saga
```

### Setup in Program.cs

```csharp
using Genocs.Saga;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSaga();

var app = builder.Build();

app.Run();
```

### Message processing usage

```csharp
var coordinator = app.Services.GetRequiredService<ISagaCoordinator>();
await coordinator.ProcessAsync(message);
```

## Behavior Notes That Affect Agent Decisions

- SagaCoordinator serializes processing per saga ID using KeyedLocker to avoid concurrent mutation of the same saga.
- SagaSeeker merges ISagaAction<TMessage> and ISagaStartAction<TMessage>, then de-duplicates by concrete type.
- SagaProcessor catches handler exceptions, sets SagaContextError, rejects saga when needed, and still persists state/log.
- Saga.Reject sets state to Rejected and throws SagaException by design.
- SagaPostProcessor executes compensation by reading saga logs and replaying messages in descending CreatedAt order.
- Initial state is created only for ISagaStartAction<TMessage> when no persisted state exists.

## Source-Accurate Capability Map

### Registration and saga discovery

- Registers the saga coordination pipeline services.
- Provides default in-memory persistence or custom persistence overrides.
- Scans loaded assemblies and registers saga interfaces with transient lifetime.

Files:

- [src/Genocs.Saga/Extensions.cs](src/Genocs.Saga/Extensions.cs)
- [src/Genocs.Saga/Builders/SagaBuilder.cs](src/Genocs.Saga/Builders/SagaBuilder.cs)

### Coordinated execution pipeline

- Orchestrates message processing through seeker, initializer, processor, and post-processor components.
- Applies per-saga locking boundary for consistent state transitions.
- Supports optional completion and rejection callbacks.

Files:

- [src/Genocs.Saga/Managers/SagaCoordinator.cs](src/Genocs.Saga/Managers/SagaCoordinator.cs)
- [src/Genocs.Saga/Managers/SagaSeeker.cs](src/Genocs.Saga/Managers/SagaSeeker.cs)
- [src/Genocs.Saga/Async/KeyedLocker.cs](src/Genocs.Saga/Async/KeyedLocker.cs)

### State initialization and persistence updates

- Reads existing saga state and prevents re-entry for rejected sagas.
- Creates default saga state and data object for start actions.
- Persists both state and log records after each message handling pass.

Files:

- [src/Genocs.Saga/Managers/SagaInitializer.cs](src/Genocs.Saga/Managers/SagaInitializer.cs)
- [src/Genocs.Saga/Managers/SagaProcessor.cs](src/Genocs.Saga/Managers/SagaProcessor.cs)
- [src/Genocs.Saga/Persistence/SagaState.cs](src/Genocs.Saga/Persistence/SagaState.cs)
- [src/Genocs.Saga/Persistence/SagaLogData.cs](src/Genocs.Saga/Persistence/SagaLogData.cs)

### Rejection and compensation flow

- Invokes custom rejection callback when saga transitions to Rejected.
- Loads persisted saga logs and invokes compensate handlers in reverse order.

Files:

- [src/Genocs.Saga/Managers/SagaPostProcessor.cs](src/Genocs.Saga/Managers/SagaPostProcessor.cs)
- [src/Genocs.Saga/ISagaAction.cs](src/Genocs.Saga/ISagaAction.cs)

### Default in-memory persistence implementation

- Stores saga state and log data in process-local lists.
- Supports read and write operations through repository/log abstractions.

Files:

- [src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs](src/Genocs.Saga/Persistence/InMemorySagaStateRepository.cs)
- [src/Genocs.Saga/Persistence/InMemorySagaLog.cs](src/Genocs.Saga/Persistence/InMemorySagaLog.cs)

## Dependencies

From [src/Genocs.Saga/Genocs.Saga.csproj](src/Genocs.Saga/Genocs.Saga.csproj):

- Microsoft.Extensions.DependencyInjection
- Scrutor

## Related Docs

- NuGet package readme: [src/Genocs.Saga/README_NUGET.md](src/Genocs.Saga/README_NUGET.md)
- Package readme: [src/Genocs.Saga/README.md](src/Genocs.Saga/README.md)
- Repository guide: [README.md](README.md)
