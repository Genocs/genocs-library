# Genocs.Core Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet
- Do not rely on repository source code access
- Prefer stable public APIs and extension methods documented here
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

`Genocs.Core` provides the foundational host bootstrap and CQRS service wiring used by Genocs-based applications.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Core` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Core runtime bootstrap and in-process CQRS registration |
| Core entry points | `AddGenocs()`, `Build()`, `UseGenocs()`, `AddDispatchers()`, `AddHandlers(...)` |

## Install

```bash
dotnet add package Genocs.Core
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commons;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();
builder.Services
    .AddDispatchers()
    .AddHandlers("MyService");

gnxBuilder.Build();

var app = builder.Build();
app.UseGenocs();
app.Run();
```

## Configuration

`Genocs.Core` does not define its own `appsettings.json` section.

What actually influences runtime behavior:

| Input | Where it comes from | Why it matters |
|---|---|---|
| Handler scan string passed to `AddHandlers(...)` | Code | Determines which loaded assemblies are scanned for command, query, and event handlers. |
| Registered startup initializers | Code/DI | Controls what `UseGenocs()` executes during startup. |
| Shared sections such as `app`, `logger`, `telemetry`, `jwt`, `webApi` | Other Genocs packages | These sections affect modules registered through the core builder, but they are not owned by `Genocs.Core`. |

Treat `Genocs.Core` as orchestration infrastructure. Configuration lives in the modules you compose on top of it.

## Decision Matrix For Agents

| If you need to... | Use |
|---|---|
| Initialize Genocs runtime services | `AddGenocs()` |
| Finalize Genocs module registrations | `IGenocsBuilder.Build()` |
| Run Genocs startup initialization in middleware pipeline | `UseGenocs()` |
| Register in-process command/query/event dispatchers | `AddDispatchers()` |
| Register handler types through assembly scan | `AddHandlers("MyService")` |

## Behavior Notes / Constraints

- Dispatcher and handler execution is in-process; this package does not provide brokered messaging.
- Handler discovery depends on assembly loading and matching scan input.
- Startup initializers run only when `UseGenocs()` is part of the request pipeline.

## Public Capability Map

- Host builder capabilities: `AddGenocs()`, `IGenocsBuilder.Build()`, `UseGenocs()`
- CQRS DI registration: `AddDispatchers()`, `AddHandlers(...)`, `AddCommandHandlers(...)`, `AddQueryHandlers(...)`, `AddEventHandlers(...)`
- Core abstractions: command, query, event dispatching contracts used by runtime packages

## Dependencies

- `Genocs.Common`
- `Scrutor`
- `Ardalis.Specification`

## Troubleshooting

1. Handlers are not being resolved at runtime.
Fix: Verify the assembly selector passed to `AddHandlers(...)` matches the deployed handler assembly name.
2. Startup logic registered through Genocs does not execute.
Fix: Ensure `UseGenocs()` is invoked after app build and before endpoint execution.
3. Command or query dispatch services are missing from DI.
Fix: Register `AddDispatchers()` before calling `builder.Build()`.


