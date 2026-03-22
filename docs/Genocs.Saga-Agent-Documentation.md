# Genocs.Saga Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

Genocs.Saga provides the saga orchestration runtime and contracts for long-running distributed workflows. It handles state-machine-style transitions, multi-step processing pipelines (seek → initialize → process → post-process), compensation for rejected flows, and saga action discovery via assembly scanning.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Saga` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Saga orchestration runtime |
| Typical startup APIs | `AddSaga` |

## Install

```bash
dotnet add package Genocs.Saga
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Saga;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSaga();

var app = builder.Build();
app.Run();
```

## Configuration

The base package does not define an `appsettings.json` section.

Configuration enters the system through the persistence integration you choose:

| Integration | Section |
|---|---|
| Default in-memory persistence | None |
| `Genocs.Saga.Integrations.MongoDB` | `sagaMongo` |
| `Genocs.Saga.Integrations.Redis` | `sagaRedis` or an explicitly named serialized section |

Everything else is registration-driven: saga discovery, handlers, and lifecycle behavior are controlled by code and DI registration rather than by base-package settings.

## Decision Matrix For Agents

| Goal | Preferred API | Why |
|---|---|---|
| Enable saga runtime with defaults | `AddSaga()` | Registers all runtime services with in-memory state and log |
| Override persistence provider | `AddSaga(saga => saga.UseSagaStateRepository<T>()...)` | Configures durable state/log storage without changing action contracts |
| Process a message through the saga pipeline | `ISagaCoordinator.ProcessAsync<TMessage>(message)` | Central runtime entry — runs the full seek/init/process/post pipeline |
| Supply completion and rejection callbacks | `ISagaCoordinator.ProcessAsync(message, onCompleted, onRejected)` | Hooks into the post-processing result without custom saga logic |
| Replace state storage | `ISagaBuilder.UseSagaStateRepository<T>()` | Accepts any `ISagaStateRepository` implementation |
| Replace log storage | `ISagaBuilder.UseSagaLog<T>()` | Accepts any `ISagaLog` implementation |

## Behavior Notes / Constraints

- Default state and log persistence are in-memory and are lost on process restart.
- Saga type discovery uses Scrutor assembly scanning over `AppDomain.CurrentDomain.GetAssemblies()` at startup.
- `ISagaStartAction<T>` must be implemented for the message that initialises a new saga; non-start messages are rejected when no saga state exists.
- Compensation runs in reverse log order through all recorded saga actions when the saga transitions to rejected state.
- `ISagaCoordinator.ProcessAsync` is transient-safe; the coordinator and pipeline services are all registered as transient.

## Public Capability Map

| Capability | Surface |
|---|---|
| Register saga runtime services | `AddSaga` on `IServiceCollection` |
| Customise persistence providers | `ISagaBuilder` |
| Coordinate message processing end-to-end | `ISagaCoordinator.ProcessAsync` |
| Saga state persistence contract | `ISagaStateRepository` |
| Saga log persistence contract | `ISagaLog` |
| Define a saga message handler | `ISagaAction<TSaga, TMessage>` |
| Define a saga-initialising handler | `ISagaStartAction<TSaga, TMessage>` |

## Dependencies

- `Microsoft.Extensions.DependencyInjection`
- `Scrutor`

## Troubleshooting

1. Saga actions are not executed for incoming messages.
Fix: Confirm that `ISagaAction<TSaga, TMessage>` or `ISagaStartAction` is implemented in a loaded assembly; verify that assembly scanning via `AppDomain.CurrentDomain.GetAssemblies()` includes the target assembly at startup.
2. Saga state is lost after application restart.
Fix: Replace the default in-memory persistence by calling `UseSagaStateRepository<T>()` and `UseSagaLog<T>()` on `ISagaBuilder` with a durable integration package such as `Genocs.Saga.Integrations.MongoDB`.
3. Compensation never executes on rejected workflows.
Fix: Verify the saga transitions to rejected state (the rejection path must be triggered in the saga action), and ensure saga log persistence is configured and writable so that the log history required for reverse compensation is available.
