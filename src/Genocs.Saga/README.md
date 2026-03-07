# Genocs.Saga

Saga pattern abstractions for distributed workflow orchestration. Supports `net10.0`, `net9.0`, and `net8.0`.

## Overview

Genocs.Saga provides a lightweight orchestration framework for implementing the saga pattern in distributed systems. Sagas coordinate multi-step workflows that can span multiple services or components. When a step fails, the saga runs compensation logic in reverse order to undo completed steps.

## Installation

```bash
dotnet add package Genocs.Saga
```

## Quick Start

### 1. Register saga services

```csharp
// In Program.cs or Startup.cs
builder.Services.AddSaga();

// Or with custom persistence (e.g. MongoDB, Redis)
builder.Services.AddSaga(saga =>
{
    saga.UseSagaStateRepository<MongoSagaStateRepository>();
    saga.UseSagaLog<MongoSagaLog>();
});
```

### 2. Define saga data and messages

```csharp
public class SagaData
{
    public bool IsStartTransaction { get; set; }
    public bool IsCompleteTransaction { get; set; }
    public bool IsSagaCompleted => IsStartTransaction && IsCompleteTransaction;
    public int TransactionValue { get; set; }
}

public record StartTransaction(string Text, int TransactionValue);
public record CompleteTransaction(string Text);
```

### 3. Implement a saga

```csharp
public class SampleSaga : Saga<SagaData>,
    ISagaStartAction<StartTransaction>,
    ISagaAction<CompleteTransaction>
{
    public Task HandleAsync(StartTransaction message, ISagaContext context)
    {
        Data.IsStartTransaction = true;
        Data.TransactionValue = message.TransactionValue;
        Complete();
        return Task.CompletedTask;
    }

    public Task HandleAsync(CompleteTransaction message, ISagaContext context)
    {
        Data.IsCompleteTransaction = true;
        if (Data.TransactionValue < 0)
            throw new Exception("Simulated failure");
        Complete();
        return Task.CompletedTask;
    }

    public Task CompensateAsync(StartTransaction message, ISagaContext context)
        => Task.CompletedTask; // Undo start logic

    public Task CompensateAsync(CompleteTransaction message, ISagaContext context)
        => Task.CompletedTask; // Undo complete logic
}
```

### 4. Process messages

```csharp
public class OrderService
{
    private readonly ISagaCoordinator _sagaCoordinator;

    public async Task StartOrderAsync(SagaId sagaId, string originator)
    {
        var context = SagaContext.Create()
            .WithSagaId(sagaId)
            .WithOriginator(originator)
            .Build();

        await _sagaCoordinator.ProcessAsync(
            new StartTransaction("Order started", 100),
            context);
    }

    public async Task CompleteOrderAsync(SagaId sagaId, string originator)
    {
        var context = SagaContext.Create()
            .WithSagaId(sagaId)
            .WithOriginator(originator)
            .Build();

        await _sagaCoordinator.ProcessAsync(
            new CompleteTransaction("Order completed"),
            onCompleted: (m, ctx) => { /* success */ return Task.CompletedTask; },
            onRejected: (m, ctx) => { /* failure */ return Task.CompletedTask; },
            context: context);
    }
}
```

## Persistence

| Persistence | Package | Usage |
|-------------|---------|-------|
| In-memory (default) | Built-in | `AddSaga()` or `UseInMemoryPersistence()` |
| MongoDB | Genocs.Saga.Integrations.MongoDB | `UseSagaStateRepository<MongoSagaStateRepository>()` |
| Redis | Genocs.Saga.Integrations.Redis | `UseSagaStateRepository<RedisSagaStateRepository>()` |

## Telemetry

Saga execution emits OpenTelemetry spans for correlation with Jaeger and other tracing backends. When using `Genocs.Telemetry`, saga spans are automatically included.

**Span hierarchy** (visible in Jaeger):

- `Saga.Process` – overall message processing
- `Saga.Execute` – per-saga execution
- `Saga.Handle` – action handling
- `Saga.Compensate` – compensation when a saga is rejected

**Span tags**: `saga.id`, `saga.type`, `saga.message.type`, `saga.state`

### Cross-service trace propagation

For sagas triggered by messages (e.g. from RabbitMQ or Kafka), propagate trace context so the saga appears in the same trace as the originating request:

```csharp
// From current Activity (e.g. in a message handler)
var context = SagaContext.Create()
    .WithSagaId(sagaId)
    .WithOriginator("orders-service")
    .WithCurrentTraceContext()
    .Build();

// Or from message headers
var context = SagaContext.Create()
    .WithSagaId(sagaId)
    .WithOriginator("orders-service")
    .WithTraceContext(traceparentFromHeaders, tracestateFromHeaders)
    .Build();
```

## API Reference

| Type | Description |
|------|-------------|
| `ISagaCoordinator` | Process messages through sagas |
| `ISaga` | Base saga contract |
| `ISagaAction<TMessage>` | Handle and compensate messages |
| `ISagaStartAction<TMessage>` | First action that creates the saga |
| `ISagaContext` | Context passed to saga actions |
| `ISagaContextBuilder` | Build saga context with metadata |
| `Saga<TData>` | Base class for sagas with typed data |
| `SagaProcessState` | Pending, Completed, Rejected |

## Support

- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library
