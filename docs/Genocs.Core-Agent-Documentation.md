# Genocs.Core Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Core is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Core |
| Project file | [src/Genocs.Core/Genocs.Core.csproj](src/Genocs.Core/Genocs.Core.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | Runtime implementation layer for Genocs.Common abstractions |
| Core themes | Builder bootstrap, CQRS dispatching, Domain primitives, Repository abstractions, Auditing, Utilities |

## Use This Package When

- You need a consistent app bootstrap flow with startup initializers.
- You want in-memory command, query, and event dispatching via DI.
- You need base Entity and AggregateRoot classes for DDD-style modeling.
- You need repository abstractions and a generic repository base implementation.
- You need shared extension utilities used across Genocs packages.

## Do Not Assume

- This package does not provide external message broker transport by itself.
- Handler auto-registration only works for assemblies currently loaded into AppDomain.
- The WebApplication overload of MapDefaultEndpoints is Development-only.

## High-Value Entry Points

### Builder and startup

- AddGenocs(WebApplicationBuilder) in [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)
- AddGenocs(IServiceCollection, IConfiguration?) in [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)
- UseGenocs(IApplicationBuilder) in [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)
- MapDefaultEndpoints(...) in [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)
- IGenocsBuilder in [src/Genocs.Core/Builders/IGenocsBuilder.cs](src/Genocs.Core/Builders/IGenocsBuilder.cs)
- GenocsBuilder implementation in [src/Genocs.Core/Builders/GenocsBuilder.cs](src/Genocs.Core/Builders/GenocsBuilder.cs)
- Startup initializer orchestrator in [src/Genocs.Core/Builders/StartupInitializer.cs](src/Genocs.Core/Builders/StartupInitializer.cs)

### CQRS registration and dispatch

- AddDispatchers and AddHandlers(project) in [src/Genocs.Core/CQRS/Commons/Extensions.cs](src/Genocs.Core/CQRS/Commons/Extensions.cs)
- AddCommandHandlers and AddInMemoryCommandDispatcher in [src/Genocs.Core/CQRS/Commands/Extensions.cs](src/Genocs.Core/CQRS/Commands/Extensions.cs)
- AddQueryHandlers and AddInMemoryQueryDispatcher in [src/Genocs.Core/CQRS/Queries/Extensions.cs](src/Genocs.Core/CQRS/Queries/Extensions.cs)
- AddEventHandlers and AddInMemoryEventDispatcher in [src/Genocs.Core/CQRS/Events/Extensions.cs](src/Genocs.Core/CQRS/Events/Extensions.cs)
- Unified IDispatcher implementation in [src/Genocs.Core/CQRS/Commons/InMemoryDispatcher.cs](src/Genocs.Core/CQRS/Commons/InMemoryDispatcher.cs)
- Concrete dispatchers:
  - [src/Genocs.Core/CQRS/Commands/Dispatchers/CommandDispatcher.cs](src/Genocs.Core/CQRS/Commands/Dispatchers/CommandDispatcher.cs)
  - [src/Genocs.Core/CQRS/Queries/Dispatchers/QueryDispatcher.cs](src/Genocs.Core/CQRS/Queries/Dispatchers/QueryDispatcher.cs)
  - [src/Genocs.Core/CQRS/Events/Dispatchers/EventDispatcher.cs](src/Genocs.Core/CQRS/Events/Dispatchers/EventDispatcher.cs)

### Domain model primitives

- Entity and Entity<TPrimaryKey> in [src/Genocs.Core/Domain/Entities/Entity.cs](src/Genocs.Core/Domain/Entities/Entity.cs)
- AggregateRoot and AggregateRoot<TPrimaryKey> in [src/Genocs.Core/Domain/Entities/AggregateRoot.cs](src/Genocs.Core/Domain/Entities/AggregateRoot.cs)
- DomainEvent base in [src/Genocs.Core/Domain/Entities/DomainEvent.cs](src/Genocs.Core/Domain/Entities/DomainEvent.cs)
- Entity lifecycle events:
  - [src/Genocs.Core/Domain/Events/EntityCreatedEvent.cs](src/Genocs.Core/Domain/Events/EntityCreatedEvent.cs)
  - [src/Genocs.Core/Domain/Events/EntityUpdatedEvent.cs](src/Genocs.Core/Domain/Events/EntityUpdatedEvent.cs)
  - [src/Genocs.Core/Domain/Events/EntityDeletedEvent.cs](src/Genocs.Core/Domain/Events/EntityDeletedEvent.cs)
- Entity helpers and errors:
  - [src/Genocs.Core/Domain/Entities/EntityExtensions.cs](src/Genocs.Core/Domain/Entities/EntityExtensions.cs)
  - [src/Genocs.Core/Domain/Entities/EntityNotFoundException.cs](src/Genocs.Core/Domain/Entities/EntityNotFoundException.cs)

### Repository and data access abstractions

- Repository interfaces in [src/Genocs.Core/Domain/Repositories/IRepository.cs](src/Genocs.Core/Domain/Repositories/IRepository.cs)
- Dapper repository contract in [src/Genocs.Core/Domain/Repositories/IDapperRepository.cs](src/Genocs.Core/Domain/Repositories/IDapperRepository.cs)
- Generic base implementation in [src/Genocs.Core/Domain/Repositories/RepositoryBase.cs](src/Genocs.Core/Domain/Repositories/RepositoryBase.cs)
- Repository mapping attributes:
  - [src/Genocs.Core/Domain/Repositories/AutoRepositoryTypesAttribute.cs](src/Genocs.Core/Domain/Repositories/AutoRepositoryTypesAttribute.cs)
  - [src/Genocs.Core/Domain/Repositories/TableMappingAttribute.cs](src/Genocs.Core/Domain/Repositories/TableMappingAttribute.cs)

### Auditing

- Auditing entities and helper folder: [src/Genocs.Core/Domain/Entities/Auditing](src/Genocs.Core/Domain/Entities/Auditing)
- Auditing helper logic: [src/Genocs.Core/Domain/Entities/Auditing/EntityAuditingHelper.cs](src/Genocs.Core/Domain/Entities/Auditing/EntityAuditingHelper.cs)
- Trail model and type:
  - [src/Genocs.Core/Domain/Entities/Auditing/Trail.cs](src/Genocs.Core/Domain/Entities/Auditing/Trail.cs)
  - [src/Genocs.Core/Domain/Entities/Auditing/TrailType.cs](src/Genocs.Core/Domain/Entities/Auditing/TrailType.cs)

### Utilities

- Collection extensions: [src/Genocs.Core/Collections/Extensions](src/Genocs.Core/Collections/Extensions)
- Generic helpers: [src/Genocs.Core/Extensions](src/Genocs.Core/Extensions)
- Framework exception base: [src/Genocs.Core/Exceptions/GenocsException.cs](src/Genocs.Core/Exceptions/GenocsException.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Bootstrap a service | AddGenocs on WebApplicationBuilder | Creates IGenocsBuilder and applies core setup |
| Run startup tasks | AddInitializer / AddInitializer<T> then UseGenocs | UseGenocs executes registered initializers |
| Register CQRS handlers by convention | AddHandlers(projectName) | Filters loaded assemblies by name match |
| Register command/query/event handlers separately | AddCommandHandlers, AddQueryHandlers, AddEventHandlers | Scans loaded assemblies for handler interfaces |
| Register in-memory dispatchers | AddDispatchers or AddInMemory*Dispatcher methods | Dispatchers are singleton services |
| Add standard health/root endpoints | MapDefaultEndpoints | WebApplication overload gated to Development |
| Model domain aggregates | AggregateRoot and DomainEvent | DomainEvents list is on AggregateRoot |
| Build custom repository | Inherit RepositoryBase<TEntity, TKey> | Override persistence-specific operations |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Core
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commons;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs();

builder.Services
    .AddDispatchers()
    .AddHandlers("MyService");

// Optional startup initializer
// genocs.AddInitializer<MyInitializer>();

genocs.Build();

var app = builder.Build();

app.UseGenocs();
app.MapDefaultEndpoints();

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- AddHandlers(projectName) and the Add*Handlers methods rely on AppDomain.CurrentDomain.GetAssemblies().
- Command and event dispatchers create async scopes and resolve handlers per call.
- QueryDispatcher has two query paths: one reflection-based for IQuery<TResult>, one generic path for TQuery.
- MapDefaultEndpoints(WebApplication) exits early outside Development.
- UseGenocs runs initializer execution synchronously from startup flow.

## Source-Accurate Capability Map

### Builder capabilities

- Exposes DI and configuration through IGenocsBuilder
- Supports duplicate registration protection via TryRegister
- Supports deferred build actions via AddBuildAction
- Supports explicit startup initializer registration

Files:

- [src/Genocs.Core/Builders/IGenocsBuilder.cs](src/Genocs.Core/Builders/IGenocsBuilder.cs)
- [src/Genocs.Core/Builders/GenocsBuilder.cs](src/Genocs.Core/Builders/GenocsBuilder.cs)
- [src/Genocs.Core/Builders/Extensions.cs](src/Genocs.Core/Builders/Extensions.cs)

### CQRS capabilities

- Registers command/query/event handlers via assembly scanning
- Dispatches commands to ICommandHandler<T>
- Dispatches queries to IQueryHandler<TQuery, TResult>
- Publishes events to all matching IEventHandler<T>
- Provides an aggregate IDispatcher for command/query/event APIs

Files:

- [src/Genocs.Core/CQRS/Commons/Extensions.cs](src/Genocs.Core/CQRS/Commons/Extensions.cs)
- [src/Genocs.Core/CQRS/Commons/InMemoryDispatcher.cs](src/Genocs.Core/CQRS/Commons/InMemoryDispatcher.cs)
- [src/Genocs.Core/CQRS/Commands/Dispatchers/CommandDispatcher.cs](src/Genocs.Core/CQRS/Commands/Dispatchers/CommandDispatcher.cs)
- [src/Genocs.Core/CQRS/Queries/Dispatchers/QueryDispatcher.cs](src/Genocs.Core/CQRS/Queries/Dispatchers/QueryDispatcher.cs)
- [src/Genocs.Core/CQRS/Events/Dispatchers/EventDispatcher.cs](src/Genocs.Core/CQRS/Events/Dispatchers/EventDispatcher.cs)

### Domain capabilities

- Identity and equality semantics through Entity base classes
- Aggregate roots with DomainEvents collection
- Domain event base type and created/updated/deleted wrappers
- Soft-delete convenience extensions and entity-not-found exception

Files:

- [src/Genocs.Core/Domain/Entities/Entity.cs](src/Genocs.Core/Domain/Entities/Entity.cs)
- [src/Genocs.Core/Domain/Entities/AggregateRoot.cs](src/Genocs.Core/Domain/Entities/AggregateRoot.cs)
- [src/Genocs.Core/Domain/Entities/DomainEvent.cs](src/Genocs.Core/Domain/Entities/DomainEvent.cs)
- [src/Genocs.Core/Domain/Entities/EntityExtensions.cs](src/Genocs.Core/Domain/Entities/EntityExtensions.cs)
- [src/Genocs.Core/Domain/Entities/EntityNotFoundException.cs](src/Genocs.Core/Domain/Entities/EntityNotFoundException.cs)

### Repository capabilities

- Ardalis.Specification-compatible repository abstractions
- Dapper query interfaces for read scenarios
- Generic repository base with common sync/async orchestration
- Optional mapping attributes for repository conventions

Files:

- [src/Genocs.Core/Domain/Repositories/IRepository.cs](src/Genocs.Core/Domain/Repositories/IRepository.cs)
- [src/Genocs.Core/Domain/Repositories/IDapperRepository.cs](src/Genocs.Core/Domain/Repositories/IDapperRepository.cs)
- [src/Genocs.Core/Domain/Repositories/RepositoryBase.cs](src/Genocs.Core/Domain/Repositories/RepositoryBase.cs)

### Auditing capabilities

- Base classes for creation/modification/full-audited entities
- Helper methods to set creator and modifier values
- Trail model for audit history shape

Files:

- [src/Genocs.Core/Domain/Entities/Auditing](src/Genocs.Core/Domain/Entities/Auditing)

### Utility capabilities

- Collection helpers including topological dependency sort
- String, object, and exception helper methods
- RSA key XML import/export helpers

Files:

- [src/Genocs.Core/Collections/Extensions](src/Genocs.Core/Collections/Extensions)
- [src/Genocs.Core/Extensions](src/Genocs.Core/Extensions)
- [src/Genocs.Core/Exceptions/GenocsException.cs](src/Genocs.Core/Exceptions/GenocsException.cs)

## Dependencies

From [src/Genocs.Core/Genocs.Core.csproj](src/Genocs.Core/Genocs.Core.csproj):

- Genocs.Common
- Spectre.Console
- Ardalis.Specification
- MediatR.Contracts
- Scrutor
- Microsoft.AspNetCore.App framework reference

## Related Docs

- NuGet package readme: [src/Genocs.Core/README_NUGET.md](src/Genocs.Core/README_NUGET.md)
- Repository guide: [README.md](README.md)
