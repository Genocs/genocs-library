# Genocs.WebApi.CQRS — Agent Reference Documentation

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request package version and configuration details.

## Purpose

`Genocs.WebApi.CQRS` connects CQRS dispatching to the Genocs WebApi endpoint flow and offers optional runtime command/event contract exposure.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.WebApi.CQRS` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | CQRS endpoint dispatch bridge |
| Main APIs | `AddInMemoryDispatcher`, `UseDispatcherEndpoints`, `Dispatch`, `UsePublicContracts` |

## Install

```bash
dotnet add package Genocs.WebApi.CQRS
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder
    .AddGenocs()
    .AddWebApi()
    .AddInMemoryDispatcher();

gnxBuilder.Build();

var app = builder.Build();

app.UseDispatcherEndpoints(endpoints => endpoints
    .Post<CreateOrderCommand>("/orders")
    .Get<GetOrderQuery, OrderDto>("/orders/{id}"));

app.UsePublicContracts(attributeRequired: false, endpoint: "/_contracts");
app.Run();

public sealed record CreateOrderCommand(Guid CustomerId, decimal Amount);
public sealed record GetOrderQuery(Guid Id);
public sealed record OrderDto(Guid Id, string Status);
```

## Configuration

This package does not define its own `appsettings.json` section.

Runtime behavior depends on host-level registrations instead:

| Dependency | Why it matters |
|---|---|
| `ICommandDispatcher` | Required for command endpoint dispatch. |
| `IQueryDispatcher` | Required for query endpoint dispatch. |
| `IEventDispatcher` | Required when using the unified dispatcher facade for event publishing. |
| `webApi` section | Influences request binding behavior inherited from `Genocs.WebApi`. |

If you need configuration-driven behavior, configure the underlying modules that this package composes rather than this package itself.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Register unified dispatcher facade | `AddInMemoryDispatcher()` |
| Build CQRS-first endpoint routes | `UseDispatcherEndpoints(...)` |
| Add CQRS dispatch to an existing endpoint builder chain | `Dispatch(...)` |
| Expose runtime command/event contract endpoint | `UsePublicContracts(...)` |

## Behavior Notes / Constraints

- `AddInMemoryDispatcher()` registers `IDispatcher` but still requires underlying command/query/event dispatchers.
- `UseDispatcherEndpoints(...)` handles routing and authorization flow similarly to WebApi endpoint setup.
- Contract endpoint content depends on currently loaded command and event types.

## Public Capability Map

- Unified dispatch facade via `IDispatcher`.
- CQRS endpoint builder flow via `IDispatcherEndpointsBuilder`.
- `HttpContext` helpers for command send and query execution.
- Public contracts middleware for command/event type listing.

## Dependencies

- `Genocs.WebApi`
- CQRS abstractions provided by Genocs packages in the host

## Troubleshooting

1. CQRS endpoints resolve but no command or query executes.
Fix: Register the underlying `ICommandDispatcher`, `IQueryDispatcher`, and `IEventDispatcher` services in DI.
2. `/_contracts` returns empty or incomplete results.
Fix: Ensure contract types are loaded at runtime and use the correct `attributeRequired` setting.
3. Route pipeline conflicts appear after enabling dispatcher endpoints.
Fix: Use one primary endpoint mapping strategy and avoid duplicate routing setup in the same app pipeline.

