# Genocs.Saga

Saga pattern abstractions for distributed workflow orchestration. Supports `net10.0`, `net9.0`, and `net8.0`.

## Overview

Genocs.Saga provides a lightweight orchestration framework for implementing the saga pattern in distributed systems. Sagas coordinate multi-step workflows that can span multiple services or components. When a step fails, the saga runs compensation logic in reverse order to undo completed steps.

Duplicate delivery handling is opt-in. If a message implements `ISagaMessageIdentity`, or the caller provides `SagaContextMetadataKeys.MessageId` in saga context metadata, the runtime will skip repeated deliveries for the same saga instance and message identity.

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

// Or disable local in-process locking and rely directly on durable-store concurrency
builder.Services.AddSaga(saga =>
{
    saga.UseSagaStateRepository<MongoSagaStateRepository>();
    saga.UseSagaLog<MongoSagaLog>();
    saga.DisableInProcessExecutionLock();
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
public record CompleteTransaction(string MessageId, string Text) : ISagaMessageIdentity;
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
            new CompleteTransaction("order-complete-1", "Order completed"),
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
| `ISagaMessageIdentity` | Opt-in message identity for duplicate-delivery handling |
| `ISagaContext` | Context passed to saga actions |
| `ISagaContextBuilder` | Build saga context with metadata |
| `Saga<TData>` | Base class for sagas with typed data |
| `SagaProcessState` | Pending, Completed, Rejected, Compensating, Compensated, CompensationFailed |

## Duplicate Delivery Handling

The baseline duplicate-delivery strategy is explicit rather than implicit:

- implement `ISagaMessageIdentity` on a message type when the message already owns a stable identity
- or set `SagaContextMetadataKeys.MessageId` on the saga context when message identity comes from transport headers

When a stable identity is present, the saga runtime records it alongside the log entry and skips repeated delivery for the same saga instance.

## Execution Locking

The saga runtime now treats local serialization as a configurable optimization rather than a correctness guarantee.

- `UseInProcessExecutionLock()` keeps the default process-local per-saga lock
- `DisableInProcessExecutionLock()` removes local serialization and relies on repository concurrency controls instead

This matters most in multi-node deployments. Durable state repositories remain the source of truth for concurrency safety.

## Compensation Lifecycle

Rejected sagas now persist compensation progress explicitly:

- `Rejected` while the original handling failure is being recorded
- `Compensating` while completed steps are replayed in reverse order
- `Compensated` when rollback finishes successfully
- `CompensationFailed` when a compensator throws and manual recovery is required

## Recovery Operations

The coordinator now exposes a baseline operator-facing recovery operation for rollback failures:

```csharp
await sagaCoordinator.RetryCompensationAsync<SampleSaga>(sagaId);
```

Use `RetryCompensationAsync<TSaga>(...)` when a saga is already in `CompensationFailed` and you want to retry the remaining rollback work. The retry operation:

- replays log entries still marked `Completed` or `CompensationFailed`
- skips entries already marked `Compensated`
- transitions the saga back through `Compensating`
- ends in `Compensated` on success or `CompensationFailed` again if a compensator still throws

## Support

- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library
