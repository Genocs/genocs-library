# Genocs.Saga Agent Reference

## Consumer Mode for Agents

- Assume `Genocs.Saga` is installed from NuGet and source code is not available at runtime.
- Prefer the public contracts in this document over inferred internal behavior.
- Treat persistence, correlation, and action discovery as explicit integration concerns.
- If a host already uses `Genocs.Saga.Integrations.MongoDB` or `Genocs.Saga.Integrations.Redis`, keep storage registration inside the `AddSaga(...)` callback.

## Purpose

`Genocs.Saga` is the core orchestration package for long-running workflows in Genocs-based applications.

It provides:

- DI registration for saga runtime services
- Message-to-saga dispatch through `ISagaCoordinator`
- Saga lifecycle primitives such as `Pending`, `Completed`, and `Rejected`
- Start-action and compensation contracts
- Context propagation, including trace metadata
- Pluggable state and log persistence contracts

It does not provide a transport, scheduler, or message broker. Your application must call the coordinator when a message or command arrives.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Saga` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Saga orchestration runtime and authoring contracts |
| Primary startup API | `IServiceCollection.AddSaga(...)` |
| Primary runtime API | `ISagaCoordinator.ProcessAsync(...)` |
| Default persistence | In-memory state and in-memory saga log |
| Built-in config section | None |

## Use This Package When

- You need to coordinate a multi-step business workflow across asynchronous messages or service boundaries.
- You want a compensation model for failed steps.
- You want the host to discover saga handlers automatically through DI scanning.
- You want to swap persistence implementations without changing saga handler code.
- You want trace context to flow into saga execution.

## Do Not Assume

- The package listens to queues, topics, or HTTP endpoints by itself.
- Passing `null` context gives stable saga correlation. By default it creates an empty context with a new saga id.
- `AddSaga(saga => { })` keeps the default in-memory persistence. It does not.
- Completion is a global terminal state that blocks all later messages. Only rejected state is blocked by the current runtime.
- Local locking is distributed. Concurrency is serialized per saga id only inside the current process.

## Install

```bash
dotnet add package Genocs.Saga
```

## 30-Second Integration

```csharp
using Genocs.Saga;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSaga();

var app = builder.Build();
app.Run();
```

This registers the saga runtime plus in-memory persistence.

If you need durable persistence, use an integration package inside the builder callback instead of the parameterless overload.

## Mental Model for Agents

Use this processing sequence when reasoning about behavior:

1. The host calls `ISagaCoordinator.ProcessAsync(message, ...)`.
2. The runtime resolves every registered saga action that handles that message type.
3. For each matching saga, the runtime resolves a saga id by calling `ISaga.ResolveId(message, context)`.
4. The runtime loads existing saga state from `ISagaStateRepository`.
5. If state does not exist, processing continues only if that handler is also an `ISagaStartAction<TMessage>`.
6. The runtime executes `HandleAsync`.
7. State and log entries are persisted.
8. If the saga is `Completed`, the completion callback runs.
9. If the saga is `Rejected`, the rejection callback runs and compensation replays logged messages in reverse order.

## Core Public APIs

### Registration

| API | Use it for |
|---|---|
| `IServiceCollection.AddSaga()` | Register the runtime with built-in in-memory state and log storage |
| `IServiceCollection.AddSaga(Action<ISagaBuilder>)` | Register the runtime and explicitly choose persistence |
| `ISagaBuilder.UseInMemoryPersistence()` | Explicitly register in-memory state and log storage |
| `ISagaBuilder.UseSagaStateRepository<TRepository>()` | Plug in a custom `ISagaStateRepository` |
| `ISagaBuilder.UseSagaLog<TSagaLog>()` | Plug in a custom `ISagaLog` |

### Runtime Entry Point

| API | Use it for |
|---|---|
| `ISagaCoordinator.ProcessAsync<TMessage>(TMessage, ISagaContext?)` | Process a message through all matching sagas |
| `ISagaCoordinator.ProcessAsync<TMessage>(TMessage, onCompleted, onRejected, ISagaContext?)` | Process a message and attach result hooks |

### Saga Authoring

| API | Use it for |
|---|---|
| `Saga` | Base class for sagas without typed state data |
| `Saga<TData>` | Base class for sagas that carry typed state data |
| `ISaga` | Core lifecycle and correlation contract |
| `ISaga<TData>` | Typed-state saga contract |
| `ISagaAction<TMessage>` | Handle and compensate a message |
| `ISagaStartAction<TMessage>` | Mark the handler that can create a missing saga instance |

### Context and Correlation

| API | Use it for |
|---|---|
| `SagaContext.Create()` | Build a context fluently |
| `ISagaContextBuilder.WithSagaId(...)` | Set the correlation id |
| `ISagaContextBuilder.WithOriginator(...)` | Set the logical caller or source system |
| `ISagaContextBuilder.WithMetadata(...)` | Attach arbitrary metadata |
| `SagaTraceContext.WithCurrentTraceContext()` | Copy the current `Activity` trace metadata |
| `SagaTraceContext.WithTraceContext(traceParent, traceState)` | Apply W3C trace headers from a message or request |

### Persistence Contracts

| API | Use it for |
|---|---|
| `ISagaStateRepository` | Read and write current saga state |
| `ISagaLog` | Read and write the chronological message log used for compensation |
| `ISagaState` | Represent the persisted lifecycle state and typed payload |
| `ISagaLogData` | Represent a persisted log entry |

## Recommended Integration Pattern

### 1. Register the runtime

Use in-memory persistence for development or tests only:

```csharp
using Genocs.Saga;

builder.Services.AddSaga();
```

Use a persistence provider when state must survive restarts:

```csharp
using Genocs.Saga;
using Genocs.Saga.Integrations.MongoDB;

builder.Services.AddSaga(saga =>
{
	saga.UseMongoPersistence(builder.Configuration);
});
```

### 2. Implement the saga

```csharp
using Genocs.Saga;

public sealed class OrderSagaData
{
	public bool Started { get; set; }
	public bool InventoryReserved { get; set; }
}

public sealed record StartOrder(Guid OrderId);
public sealed record ReserveInventory(Guid OrderId);

public sealed class OrderSaga : Saga<OrderSagaData>,
	ISagaStartAction<StartOrder>,
	ISagaAction<ReserveInventory>
{
	public Task HandleAsync(StartOrder message, ISagaContext context)
	{
		Data.Started = true;
		Complete();
		return Task.CompletedTask;
	}

	public Task CompensateAsync(StartOrder message, ISagaContext context)
		=> Task.CompletedTask;

	public Task HandleAsync(ReserveInventory message, ISagaContext context)
	{
		Data.InventoryReserved = true;
		Complete();
		return Task.CompletedTask;
	}

	public Task CompensateAsync(ReserveInventory message, ISagaContext context)
		=> Task.CompletedTask;
}
```

Authoring rules that matter:

- Use `Saga` or `Saga<TData>` as the base class.
- Implement `ISagaStartAction<TMessage>` on the message that is allowed to create a new saga instance.
- Implement `ISagaAction<TMessage>` for every additional message type the saga should process.
- Call `Complete()` or `Reject(...)` inside handlers to declare the outcome for the current processing pass.

### 3. Build a correlation context

```csharp
using Genocs.Saga;

var context = SagaContext.Create()
	.WithSagaId(orderId.ToString())
	.WithOriginator("orders-api")
	.Build();
```

For trace propagation:

```csharp
var context = SagaContext.Create()
	.WithSagaId(orderId.ToString())
	.WithOriginator("orders-worker")
	.WithCurrentTraceContext()
	.Build();
```

Or from headers:

```csharp
var context = SagaContext.Create()
	.WithSagaId(orderId.ToString())
	.WithOriginator("orders-worker")
	.WithTraceContext(traceParentHeader, traceStateHeader)
	.Build();
```

### 4. Dispatch a message

```csharp
public sealed class OrderOrchestrator
{
	private readonly ISagaCoordinator _sagaCoordinator;

	public OrderOrchestrator(ISagaCoordinator sagaCoordinator)
	{
		_sagaCoordinator = sagaCoordinator;
	}

	public Task StartAsync(StartOrder message, ISagaContext context)
		=> _sagaCoordinator.ProcessAsync(message, context);

	public Task ReserveAsync(ReserveInventory message, ISagaContext context)
		=> _sagaCoordinator.ProcessAsync(
			message,
			onCompleted: (_, _) => Task.CompletedTask,
			onRejected: (_, failedContext) =>
			{
				var error = failedContext.SagaContextError?.Exception;
				return Task.CompletedTask;
			},
			context: context);
}
```

## Configuration

The base package has no dedicated `appsettings.json` section.

Configuration comes from the persistence provider you install:

| Provider | Package | Typical config source |
|---|---|---|
| Built-in in-memory | `Genocs.Saga` | None |
| MongoDB | `Genocs.Saga.Integrations.MongoDB` | `sagaMongo` section or explicit options |
| Redis | `Genocs.Saga.Integrations.Redis` | `sagaRedis` section or explicit options |

## Decision Matrix for Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Start quickly with defaults | `AddSaga()` | Best for local development and tests |
| Use durable persistence | `AddSaga(saga => saga.UseMongoPersistence(...))` or `UseRedisPersistence(...)` | Keep provider registration inside the callback |
| Use a custom storage implementation | `UseSagaStateRepository<T>()` plus `UseSagaLog<T>()` | Register both state and log components |
| Process a saga message | `ISagaCoordinator.ProcessAsync(message, context)` | The package does not poll transports on its own |
| Attach success and failure hooks | `ISagaCoordinator.ProcessAsync(message, onCompleted, onRejected, context)` | Rejection hook runs before compensation |
| Correlate messages to an existing saga | `SagaContext.Create().WithSagaId(...).WithOriginator(...).Build()` | Strongly preferred over `null` context |
| Propagate distributed trace data | `WithCurrentTraceContext()` or `WithTraceContext(...)` | Adds W3C metadata to saga context |
| Start a new saga from a message | Implement `ISagaStartAction<TMessage>` | Non-start actions cannot create state |
| Override default saga correlation | Override `ResolveId(object, ISagaContext)` | Needed when correlation should come from the message body instead of `context.SagaId` |

## Behavior Notes That Change Integration Decisions

- `AddSaga()` with no callback registers in-memory state and log storage automatically.
- `AddSaga(saga => { ... })` does not add default persistence. If the callback is empty, the runtime is registered without `ISagaStateRepository` and `ISagaLog` implementations.
- Saga discovery scans assemblies already loaded in `AppDomain.CurrentDomain` at registration time. Keep saga classes in assemblies that are referenced and loaded by the host.
- Every discovered saga type that implements the incoming message contract is processed. If multiple saga classes handle the same message type, they all run.
- Matching sagas are processed concurrently, while each individual saga id is locally serialized through an in-process keyed lock.
- The default `ResolveId(...)` implementation uses `context.SagaId`. If you omit context and do not override `ResolveId`, each call effectively uses a new id from `SagaContext.Empty`.
- `SagaContext.Create().Build()` requires both `SagaId` and `Originator`. Missing either value throws `InvalidOperationException`.
- Context metadata keys must be unique. Duplicate keys throw `SagaException` when the context is created.
- If state does not exist and the handler is not an `ISagaStartAction<TMessage>`, the runtime skips that saga silently for that message.
- If a handler throws, the runtime stores the exception in `ISagaContext.SagaContextError`, rejects the saga if needed, persists state and log entries, then invokes rejection handling.
- Compensation uses the persisted saga log in reverse chronological order and calls `CompensateAsync` for each recorded message.
- A rejected saga is not re-initialized for later messages. A completed saga can still be loaded again by the current runtime.
- The package emits `Activity` spans for processing, execution, handling, and compensation. No extra package configuration is required, but your host must have tracing configured to export them.

## Observability Notes

When tracing is enabled in the host, saga execution emits spans with the activity source name `Genocs.Saga`.

Expected activity names:

- `Saga.Process`
- `Saga.Execute`
- `Saga.Handle`
- `Saga.Compensate`

Useful tags include:

- `saga.id`
- `saga.type`
- `saga.message.type`
- `saga.state`
- `error.type`
- `error.message`

## Public Capability Map

| Capability area | Public surface |
|---|---|
| Runtime registration | `AddSaga`, `ISagaBuilder` |
| Message execution | `ISagaCoordinator.ProcessAsync(...)` |
| Saga implementation | `Saga`, `Saga<TData>`, `ISaga`, `ISaga<TData>`, `ISagaAction<TMessage>`, `ISagaStartAction<TMessage>` |
| Correlation context | `SagaContext`, `ISagaContext`, `ISagaContextBuilder`, `ISagaContextMetadata` |
| Trace propagation | `SagaTraceContext` |
| Persistence abstraction | `ISagaStateRepository`, `ISagaLog`, `ISagaState`, `ISagaLogData` |
| Lifecycle state | `SagaProcessState`, `SagaException`, `SagaContextError` |

## Dependencies

Direct package dependencies:

- `Microsoft.Extensions.DependencyInjection`
- `Scrutor`

## Related Packages

- `Genocs.Saga.Integrations.MongoDB` for durable MongoDB-backed state and log storage
- `Genocs.Saga.Integrations.Redis` for durable Redis-backed state and log storage
- `Genocs.Telemetry` if the host standardizes OpenTelemetry setup across Genocs packages

## Troubleshooting

1. Messages reach the host, but no saga runs.
Fix: Confirm the saga class implements `ISagaAction<TMessage>` or `ISagaStartAction<TMessage>`, inherits from `Saga` or `Saga<TData>`, and lives in an assembly that is already loaded when `AddSaga(...)` executes.

2. The runtime throws because `ISagaStateRepository` or `ISagaLog` is missing.
Fix: Use `AddSaga()` for the default in-memory setup, or register both persistence pieces inside the `AddSaga(saga => { ... })` callback. Do not pass an empty callback.

3. Follow-up messages create new saga instances instead of continuing the existing workflow.
Fix: Pass a context built with the same `SagaId`, or override `ResolveId(...)` to derive the id from the message payload.

4. A message for an existing workflow appears to be ignored.
Fix: If the saga state does not exist yet, only a handler implementing `ISagaStartAction<TMessage>` can create it. If the saga is already rejected, later messages are ignored by the runtime.

5. Compensation never runs.
Fix: Compensation runs only when the saga state becomes `Rejected`. Ensure the handler throws or calls `Reject(...)`, and verify the saga log implementation is registered and writable.

6. Building the context throws before processing starts.
Fix: Provide both `SagaId` and `Originator`, and avoid duplicate metadata keys when calling `WithMetadata(...)`.

7. Traces are missing from distributed telemetry.
Fix: Configure tracing in the host and propagate trace metadata into the saga context with `WithCurrentTraceContext()` or `WithTraceContext(...)`.

## Version Awareness

The current repository changelog shows recent unreleased documentation standardization work for NuGet readmes, but no package-specific API changes were identified for `Genocs.Saga` in the local workspace state.
