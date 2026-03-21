# Genocs.WebApi.CQRS — Agent Reference Documentation

> **Format**: AI-optimized agent reference. Structured for rapid decision-making. All capability claims are
> linked to actual source files. Do not infer capabilities not listed here.

---

## 1. Purpose

`Genocs.WebApi.CQRS` wires the CQRS abstractions from `Genocs.Common` (`ICommand`, `IQuery<T>`, `IEvent`)
into the `Genocs.WebApi` endpoint-builder DSL. It provides:

- `IDispatcher` / `InMemoryDispatcher` — a single façade that delegates to the three dispatchers
  (`ICommandDispatcher`, `IEventDispatcher`, `IQueryDispatcher`).
- `IDispatcherEndpointsBuilder` / `DispatcherEndpointsBuilder` — a typed alternative to `IEndpointsBuilder`
  that auto-dispatches bound commands and queries via the in-memory dispatcher.
- `UseDispatcherEndpoints` middleware helper — wires routing + auth + the dispatcher-flavoured endpoint DSL in
  one call.
- `UsePublicContracts` — exposes a diagnostic JSON endpoint that lists all known `ICommand` and `IEvent`
  types in the process (useful for tooling and contract testing).
- `HttpContext` extension methods — `SendAsync<T>` and `QueryAsync<TResult>` shortcuts.

**Package ID**: `Genocs.WebApi.CQRS`  
**NuGet config section**: none (no config section; pure DI/middleware)

---

## 2. Quick Facts

| Property | Value |
|---|---|
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Config section key | — (none) |
| Registration guard | none — `AddInMemoryDispatcher` is not idempotent |
| Dispatcher type | In-process / in-memory (delegates to injected dispatchers) |
| Public contracts endpoint | `/_contracts` (default, configurable) |
| Requires | `Genocs.WebApi` (which requires `Genocs.Core`, `Genocs.Common`) |
| Source file | [`src/Genocs.WebApi.CQRS/Extensions.cs`](../src/Genocs.WebApi.CQRS/Extensions.cs) |

---

## 3. Use When

- You want to register HTTP endpoints where the framework automatically dispatches bound request bodies as
  `ICommand` or `IQuery<T>` objects — removing boilerplate dispatcher injection in every controller/handler.
- You have `ICommandDispatcher`, `IEventDispatcher`, and `IQueryDispatcher` already registered (e.g. via
  `Genocs.Core` CQRS registration) and want a single `IDispatcher` façade to inject.
- You want to expose a `/_contracts` endpoint that enumerates all commands and events in the assembly —
  useful for agentic tooling, contract testing, and documentation generation.
- You want to mix typed CQRS endpoints (`Post<TCommand>`, `Get<TQuery, TResult>`) with raw handler endpoints
  (`Post(path, ctx => ...)`) in the same endpoint builder chain.

---

## 4. Do Not Assume

- **`AddInMemoryDispatcher` is not idempotent.** It calls `AddSingleton<IDispatcher, InMemoryDispatcher>`.
  Calling it twice registers two singletons; only the first will be resolved.
- **`InMemoryDispatcher` does NOT provide its own command/query dispatchers.** It delegates entirely to
  `ICommandDispatcher`, `IEventDispatcher`, `IQueryDispatcher` — those must be registered separately (e.g.
  via `Genocs.Core`'s CQRS setup).
- **`UseDispatcherEndpoints` replaces `UseEndpoints`** from `Genocs.WebApi`. Do not call both; they both call
  `app.UseRouting()` and `app.UseEndpoints(...)` internally.
- **`UsePublicContracts` reflects all loaded assemblies at app startup.** Only types implementing `ICommand`
  or `IEvent` that are visible to `AppDomain.CurrentDomain.GetAssemblies()` at that moment are indexed. Types
  loaded lazily after startup are NOT included.
- **Duplicate command/event names throw.** If two types share the same `.Name` (regardless of namespace),
  `UsePublicContracts` throws `InvalidOperationException` on startup.
- **`attributeRequired = true` (default)** means only types decorated with the `attributeType` passed to
  `UsePublicContracts` will be included. Pass `attributeRequired: false` to include all `ICommand`/`IEvent`
  types unconditionally.
- **`DispatcherEndpointsBuilder` wraps `IEndpointsBuilder`**; it's not a standalone builder — it requires an
  existing `IEndpointsBuilder` instance (obtained via `UseDispatcherEndpoints` or `endpoints.Dispatch(...)`).
- **The `beforeDispatch` / `afterDispatch` hooks** in typed endpoints are synchronous with the dispatch
  pipeline. Exceptions thrown in hooks will propagate up.

---

## 5. High-Value Entry Points

```
Extensions.cs → AddInMemoryDispatcher(IGenocsBuilder)
Extensions.cs → UseDispatcherEndpoints(IApplicationBuilder, Action<IDispatcherEndpointsBuilder>, ...)
Extensions.cs → Dispatch(IEndpointsBuilder, Func<IDispatcherEndpointsBuilder, ...>)
Extensions.cs → UsePublicContracts<T>(IApplicationBuilder, endpoint)
Extensions.cs → UsePublicContracts(IApplicationBuilder, bool attributeRequired, endpoint)
Extensions.cs → SendAsync<T>(HttpContext, T command)
Extensions.cs → QueryAsync<TResult>(HttpContext, IQuery<TResult>)
IDispatcher.cs → IDispatcher                                          ← unified façade interface
InMemoryDispatcher.cs → InMemoryDispatcher                            ← singleton implementation
IDispatcherEndpointsBuilder.cs → IDispatcherEndpointsBuilder
Builders/DispatcherEndpointsBuilder.cs → DispatcherEndpointsBuilder
Middlewares/PublicContractsMiddleware.cs → PublicContractsMiddleware
```

---

## 6. Decision Matrix

| Goal | API to use |
|---|---|
| Register the unified IDispatcher | `builder.AddInMemoryDispatcher()` |
| Add CQRS-dispatching HTTP endpoints | `app.UseDispatcherEndpoints(endpoints => endpoints.Post<MyCmd>("/cmd").Get<MyQuery, Result>("/q"))` |
| Add CQRS endpoints in an existing endpoint chain | `endpoints.Dispatch(b => b.Post<MyCmd>("/cmd"))` |
| Expose command/event contract list at `/_contracts` | `app.UsePublicContracts<PublicContractAttribute>()` |
| Include all commands/events (no attribute filter) | `app.UsePublicContracts(attributeRequired: false)` |
| Dispatch command from HttpContext | `await context.SendAsync(myCommand)` |
| Dispatch query from HttpContext | `var result = await context.QueryAsync(myQuery)` |
| Intercept command before/after dispatch | Use `beforeDispatch` / `afterDispatch` parameters in typed endpoint methods |

---

## 7. Minimal Integration Recipe

### 7.1 Program.cs — Registration

```csharp
IGenocsBuilder gnxBuilder = builder
    .AddGenocs()
    .AddWebApi()
    .AddInMemoryDispatcher();   // registers IDispatcher singleton

gnxBuilder.Build();
```

Note: `ICommandDispatcher`, `IQueryDispatcher`, `IEventDispatcher` must be registered independently
(e.g. via Genocs.Core's `AddCommandHandlers()` / `AddQueryHandlers()` or your own registrations).

### 7.2 Program.cs — Middleware (CQRS endpoint builder)

```csharp
app.UseDispatcherEndpoints(endpoints => endpoints
    .Post<CreateOrderCommand>("/orders", auth: true)
    .Get<GetOrderQuery, OrderDto>("/orders/{id}", auth: true)
    .Delete<CancelOrderCommand>("/orders/{id}", auth: true));
```

### 7.3 Mixing with raw endpoints

```csharp
app.UseDispatcherEndpoints(endpoints => endpoints
    .Get("/health", ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; })
    .Post<CreateOrderCommand>("/orders"));
```

### 7.4 Public contracts

```csharp
// Only exposes types marked [PublicContract]
app.UsePublicContracts<PublicContractAttribute>();

// Exposes all ICommand / IEvent types
app.UsePublicContracts(attributeRequired: false, endpoint: "/_contracts");
```

### 7.5 Inject IDispatcher directly

```csharp
public class MyService(IDispatcher dispatcher)
{
    public Task Handle(MyCommand cmd) => dispatcher.SendAsync(cmd);
}
```

---

## 8. Behavior Notes

- **`BuildCommandContext<T>`** (private helper in `DispatcherEndpointsBuilder`): resolves `ICommandDispatcher`
  from `HttpContext.RequestServices` at request time, then calls `beforeDispatch` → `SendAsync` →
  `afterDispatch`. If `ICommandDispatcher` is null (not registered), the dispatch is silently skipped — no
  exception is thrown.
- **Typed `Get<TQuery, TResult>`**: if no `afterDispatch` is provided and the query result is null, responds
  with HTTP 404 automatically. If the result is non-null, serializes and writes it via
  `ctx.Response.WriteJsonAsync(result)`.
- **`PublicContractsMiddleware`** initialises contracts exactly once via `Interlocked.Exchange` — thread-safe,
  first-call wins. The static `_serializedContracts` string is computed once and reused for all subsequent
  requests.
- **`PublicContractsMiddleware` contract serialization** uses `System.Text.Json` with camelCase property name
  policy and `JsonStringEnumConverter` (camelCase) — the output is always prettified JSON.
- **`UsePublicContracts` defaults**: endpoint = `"/_contracts"`, `attributeType = typeof(PublicContractAttribute)`,
  `attributeRequired = true`.

---

## 9. Source-Accurate Capability Map

| Capability | Source Location |
|---|---|
| `IDispatcher` interface | [`IDispatcher.cs`](../src/Genocs.WebApi.CQRS/IDispatcher.cs) |
| `InMemoryDispatcher` | [`InMemoryDispatcher.cs`](../src/Genocs.WebApi.CQRS/InMemoryDispatcher.cs) |
| `AddInMemoryDispatcher` | [`Extensions.cs`](../src/Genocs.WebApi.CQRS/Extensions.cs) |
| `UseDispatcherEndpoints` | [`Extensions.cs`](../src/Genocs.WebApi.CQRS/Extensions.cs) |
| `Dispatch` (inline DSL) | [`Extensions.cs`](../src/Genocs.WebApi.CQRS/Extensions.cs) |
| `UsePublicContracts` | [`Extensions.cs`](../src/Genocs.WebApi.CQRS/Extensions.cs) |
| `SendAsync` / `QueryAsync` (HttpContext) | [`Extensions.cs`](../src/Genocs.WebApi.CQRS/Extensions.cs) |
| `IDispatcherEndpointsBuilder` | [`IDispatcherEndpointsBuilder.cs`](../src/Genocs.WebApi.CQRS/IDispatcherEndpointsBuilder.cs) |
| `DispatcherEndpointsBuilder` | [`Builders/DispatcherEndpointsBuilder.cs`](../src/Genocs.WebApi.CQRS/Builders/DispatcherEndpointsBuilder.cs) |
| `PublicContractsMiddleware` | [`Middlewares/PublicContractsMiddleware.cs`](../src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs) |

---

## 10. Dependencies

| Dependency | Role |
|---|---|
| `Genocs.WebApi` | Provides `IEndpointsBuilder`, `WebApiEndpointDefinitions`, `IGenocsBuilder` |
| `Genocs.Common` | Provides `ICommand`, `IQuery<T>`, `IEvent`, `ICommandDispatcher`, `IQueryDispatcher` |
| `Genocs.Core` | Provides `IGenocsBuilder`, `IEventDispatcher` |
| `Microsoft.AspNetCore.App` (framework ref) | ASP.NET Core pipeline primitives |

---

## 11. Related Docs

- NuGet package readme: [src/Genocs.WebApi.CQRS/README_NUGET.md](src/Genocs.WebApi.CQRS/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.WebApi.CQRS-Agent-Documentation.md](docs/Genocs.WebApi.CQRS-Agent-Documentation.md)
- Related: [Genocs.WebApi-Agent-Documentation.md](docs/Genocs.WebApi-Agent-Documentation.md) — base endpoint DSL that `DispatcherEndpointsBuilder` wraps
- Related: [Genocs.Core-Agent-Documentation.md](docs/Genocs.Core-Agent-Documentation.md) — CQRS abstractions source (`ICommandDispatcher`, `IEventDispatcher`)
