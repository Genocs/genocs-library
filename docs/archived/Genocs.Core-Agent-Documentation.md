# Genocs.Core Agent Reference

## Agent Operating Mode

- Assume `Genocs.Core` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, interfaces, and base classes as the only safe API surface.
- If package composition is unclear, ask which companion Genocs packages are installed before generating code that depends on them.
- Prefer the smallest working integration path. Do not invent infrastructure that this package does not provide.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Core` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Runtime foundation for Genocs applications |
| Main value | Host bootstrap, startup initialization, in-process CQRS wiring, domain base classes, repository foundations |
| Not included | HTTP endpoints, brokered messaging, database provider implementations, automatic persistence |

## What This Package Is For

Use `Genocs.Core` when you need to:

- bootstrap a Genocs-based host with `AddGenocs()`
- run startup initialization logic through `UseGenocs()`
- register in-process command, query, and event handlers
- use concrete domain base classes such as `Entity`, `AggregateRoot`, and audited entities
- build repository implementations on top of Genocs contracts

## What This Package Does Not Do By Itself

Do not assume `Genocs.Core` can:

- expose REST endpoints or Minimal API features
- connect to MongoDB, SQL Server, Redis, RabbitMQ, or any other infrastructure service
- publish messages to an external broker
- auto-discover assemblies that are not already loaded into the process
- persist entities without a concrete repository or persistence package
- populate audit fields automatically in every application flow

## Safe Default Mental Model

Treat `Genocs.Core` as four things:

1. A startup shell around `IGenocsBuilder`
2. An in-process CQRS registration and dispatch layer
3. A set of concrete DDD base classes
4. A repository and utility foundation for companion packages or custom infrastructure

If a user asks for runtime behavior outside those boundaries, identify the missing package first.

## Fast Start Recipes

### Recipe 1: Bootstrap A Web Host

Use this when the project has `WebApplicationBuilder`.

```csharp
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs();
genocs.Build();

var app = builder.Build();
app.UseGenocs();
app.Run();
```

Effect:
- registers core runtime services
- binds the shared `app` section into `AppOptions`
- adds memory cache
- adds health checks with a built-in `self` liveness check
- creates the startup initializer pipeline

### Recipe 2: Add In-Process CQRS With Narrower Scanning

Use this when handlers live in assemblies whose names contain a known project string.

```csharp
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commons;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs();

builder.Services
    .AddDispatchers()
    .AddHandlers("MyService");

genocs.Build();

var app = builder.Build();
app.UseGenocs();
app.Run();
```

Use this when you want predictable registration from specific loaded assemblies.

### Recipe 3: Add CQRS Using Builder Extensions

Use this when relevant handler assemblies are already loaded and broad AppDomain scanning is acceptable.

```csharp
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddCommandHandlers()
    .AddEventHandlers()
    .AddQueryHandlers()
    .AddInMemoryCommandDispatcher()
    .AddInMemoryEventDispatcher()
    .AddInMemoryQueryDispatcher();

genocs.Build();

var app = builder.Build();
app.UseGenocs();
app.Run();
```

Use this when the application already composes modules through `IGenocsBuilder`.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddGenocs()` | Initialize the core runtime | Registers `AppOptions`, health checks, memory cache, `IServiceId`, and startup initialization support | Assuming it also registers CQRS handlers or external integrations |
| `IGenocsBuilder.Build()` | Finalize queued Genocs build actions | Executes deferred actions against a built service provider | Forgetting to call it before `builder.Build()` |
| `UseGenocs()` | Run startup initializers | Resolves `IStartupInitializer` and executes `InitializeAsync()` | Expecting initializers to run automatically |
| `AddInitializer(IInitializer)` | Queue a known initializer instance | Adds the instance to the startup initializer pipeline during `Build()` | Registering after the host is already built |
| `AddInitializer<TInitializer>()` | Queue a DI-resolved initializer type | Requires `TInitializer` to already be registered in DI | Assuming Genocs.Core auto-registers the initializer type |
| `AddDispatchers()` | Register unified in-memory dispatchers | Registers `IDispatcher`, `ICommandDispatcher`, `IQueryDispatcher`, `IEventDispatcher` as singletons | Expecting it to also register handlers |
| `AddHandlers("Project")` | Register handlers from matching loaded assemblies | Uses assembly-name substring matching and registers handlers as scoped services | Passing the wrong project selector |
| `AddCommandHandlers()` | Register command handlers through AppDomain scanning | Registers handlers as transient services | Assuming it limits scanning to one project |
| `AddQueryHandlers()` | Register query handlers through AppDomain scanning | Registers handlers as transient services | Forgetting the query dispatcher registration |
| `AddEventHandlers()` | Register event handlers through AppDomain scanning | Registers handlers as transient services | Assuming events are brokered externally |
| `MapDefaultEndpoints()` | Expose root and health endpoints | WebApplication overload is development-only; IApplicationBuilder overload is not | Exposing endpoints unintentionally in production |

## Choosing A Registration Style

| Situation | Prefer |
|---|---|
| You already use `IGenocsBuilder` for host composition | Builder extensions such as `AddCommandHandlers()` |
| You want one call for all dispatchers | `AddDispatchers()` |
| You need narrower handler scanning by assembly name | `AddHandlers("ProjectName")` |
| You only need command dispatching | `AddInMemoryCommandDispatcher()` plus command handler registration |
| You want the lowest-risk setup for an unknown codebase | `AddGenocs()` plus explicit dispatcher and handler registration |

## Public Capability Map

### Host Bootstrap

- `IGenocsBuilder`
- `GenocsBuilder`
- `AddGenocs(WebApplicationBuilder)`
- `AddGenocs(IServiceCollection, IConfiguration?)`
- `UseGenocs()`
- `GetOptions<TModel>(sectionName)`
- `MapDefaultEndpoints()`

### CQRS Runtime

- `AddDispatchers()`
- `AddHandlers(string project)`
- `AddCommandHandlers()`
- `AddQueryHandlers()`
- `AddEventHandlers()`
- `AddInMemoryCommandDispatcher()`
- `AddInMemoryQueryDispatcher()`
- `AddInMemoryEventDispatcher()`
- `IDispatcher`, `ICommandDispatcher`, `IQueryDispatcher`, `IEventDispatcher`

### Domain Base Types

- `Entity`
- `Entity<TPrimaryKey>`
- `AggregateRoot`
- `AggregateRoot<TPrimaryKey>`
- `DomainEvent`
- `EntityCreatedEvent<TEntity>`
- `EntityUpdatedEvent<TEntity>`
- `EntityDeletedEvent<TEntity>`

### Auditing

- `CreationAuditedEntity`
- `AuditedEntity`
- `FullAuditedEntity`
- matching aggregate-root variants
- `EntityAuditingHelper`
- `IAuditService`

### Repository Foundations

- `IRepository<T>`
- `IReadRepository<T>`
- `IRepositoryWithEvents<T>`
- `RepositoryBase<TEntity, TKey>`
- `IDapperRepository`
- `TableMappingAttribute`
- `AutoRepositoryTypesAttribute`

### Utilities And Exceptions

- collection, dictionary, enumerable, string, object, and exception extensions
- `Encryption` RSA XML helpers
- `GenocsException`
- `EntityNotFoundException`

## Configuration Ownership

`Genocs.Core` reads the shared `app` configuration section via `AppOptions`.

```json
{
  "app": {
    "enabled": true,
    "name": "BookStore",
    "service": "bookstore-api",
    "instance": "bookstore-api-01",
    "version": "1.0.0",
    "displayBanner": true,
    "displayVersion": true
  }
}
```

What this affects:

- startup banner display
- service name and version presentation
- root endpoint response content through `MapDefaultEndpoints()`

What it does not affect by itself:

- database connections
- JWT configuration
- OpenAPI
- web endpoint conventions
- external messaging

Those concerns belong to companion packages.

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume handlers exist just because dispatchers are registered.
2. Do not assume assembly scanning will load missing assemblies.
3. Do not assume `AddInitializer<T>()` works unless the initializer type is registered in DI.
4. Do not assume audited fields populate automatically without application or persistence logic.
5. Do not assume repository interfaces imply a concrete storage implementation.
6. Do not assume `MapDefaultEndpoints()` is always safe in production.
7. Do not assume event dispatch means external integration messaging.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Core`, answer these questions:

1. Is the host using `WebApplicationBuilder` or only `IServiceCollection`?
2. Are handler assemblies already loaded at runtime?
3. Should handler scanning be broad or limited to one project name?
4. Is startup initialization required?
5. Are external packages such as `Genocs.WebApi`, `Genocs.Persistence.MongoDB`, `Genocs.Persistence.EFCore`, or `Genocs.Messaging.*` also installed?
6. Is exposing health or root endpoints acceptable in the target environment?

If any answer is unknown, prefer conservative code and ask for the missing detail.

## Common Tasks And Safe Responses

### Task: "Set up CQRS"

Safe response:
- register dispatchers explicitly
- register handlers explicitly or via a known assembly selector
- do not promise external messaging

### Task: "Add repository support"

Safe response:
- model repository interfaces or base classes
- ask which persistence package is installed before generating storage code

### Task: "Add startup initialization"

Safe response:
- create an `IInitializer`
- register it in DI if using `AddInitializer<T>()`
- call `UseGenocs()` in the built app pipeline

### Task: "Enable health endpoints"

Safe response:
- use `MapDefaultEndpoints()` deliberately
- mention the production exposure difference between the two overloads

## Failure Modes And Troubleshooting

1. Command dispatch fails with missing handler resolution.
Fix: Register command handlers and verify the handler assembly is loaded.

2. Query dispatch works in one overload but not another.
Fix: Ensure the exact handler interface is registered for the query type being dispatched.

3. Startup logic never runs.
Fix: Call `IGenocsBuilder.Build()` before building the host, then call `UseGenocs()` after building it.

4. `AddInitializer<T>()` throws because the service is missing.
Fix: Register `TInitializer` in DI before calling `Build()`.

5. No handlers are discovered with `AddHandlers("Project")`.
Fix: Verify the target assembly name contains the provided project string and that the assembly is already loaded.

6. Root or health endpoints appear in an environment where they should not.
Fix: Re-check which `MapDefaultEndpoints()` overload was used and gate endpoint mapping explicitly if needed.

7. Audit fields remain null or default.
Fix: Populate them in application logic, middleware, or persistence infrastructure. Genocs.Core only provides the base types and helpers.

## Related Packages To Ask About

- `Genocs.Common`
- `Genocs.WebApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Persistence.MongoDB`
- `Genocs.Persistence.EFCore`
- `Genocs.Messaging`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Telemetry`
- `Genocs.Auth`

## One-Line Recommendation For Agents

If you only know that `Genocs.Core` is installed, generate host bootstrap, in-process CQRS wiring, domain base types, and startup initializer code only. Ask before generating storage, HTTP, authentication, or messaging infrastructure.