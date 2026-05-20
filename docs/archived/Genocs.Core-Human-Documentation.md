# Genocs.Core Library

## Overview

**Genocs.Core** is the runtime foundation package for Genocs-based applications. It turns the contracts from `Genocs.Common` into executable application behavior by providing a host bootstrap builder, startup initializers, in-memory CQRS dispatchers, concrete domain entity and aggregate base classes, repository foundations, auditing helpers, and reusable utility extensions. Use this package when you want to compose a Genocs application host, model rich domain objects, and wire command, query, and event handlers without choosing infrastructure adapters up front.

[![NuGet](https://img.shields.io/nuget/v/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core/)

## Target Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0

## Key Design Principles

The library is designed with the following principles in mind:

- **Runtime-First Composition**: Provide concrete host and dispatching behavior on top of `Genocs.Common` contracts.
- **Convention-Based Startup**: Favor extension methods, assembly scanning, and a single builder abstraction to reduce application bootstrap code.
- **DDD-Oriented Foundations**: Supply entity, aggregate root, domain event, repository, and auditing primitives for domain-centric services.
- **Incremental Adoption**: Allow teams to start with in-process dispatching and later compose persistence, messaging, and web packages around the same abstractions.
- **Host Integration With Minimal Friction**: Build on `Microsoft.Extensions.DependencyInjection` and ASP.NET Core primitives rather than introducing a custom runtime model.

## Core Components

### 1. Application Builder and Bootstrap Pipeline

Genocs.Core defines the runtime entry point for Genocs applications through `IGenocsBuilder`, `GenocsBuilder`, and host integration extensions.

- **`IGenocsBuilder`**: Central builder contract exposing `Services`, `Configuration`, `WebApplicationBuilder`, build actions, and startup initializer registration.
- **`GenocsBuilder`**: Concrete builder implementation used by `AddGenocs(...)`.
- **`AddGenocs(this WebApplicationBuilder)`**: Creates an `IGenocsBuilder`, binds `AppOptions`, adds health checks, memory cache, and a singleton `IServiceId`.
- **`AddGenocs(this IServiceCollection, IConfiguration?)`**: Enables the same runtime setup outside a `WebApplicationBuilder` host.
- **`UseGenocs(this IApplicationBuilder)`**: Executes the registered `IStartupInitializer` pipeline.
- **`AddCoreDiagnostics(...)`**: Opt-in diagnostics for startup and CQRS registration events.

For `IServiceCollection` hosts, configuration resolution is deterministic and does not create temporary service providers: Genocs uses the explicit `IConfiguration` argument first, then any pre-registered `IConfiguration` singleton, otherwise an empty configuration root.

**Key Features:**
- Unified bootstrap entry point for web hosts and plain service collections
- Deferred build actions executed through `IGenocsBuilder.Build()`
- Built-in registration of `IStartupInitializer`
- Default health check setup and in-memory caching
- `AppOptions`-driven startup banner support via Spectre.Console
- Optional diagnostics state (`CoreDiagnosticsState`) for handler discovery and startup initializer visibility

**Example Use Cases:**
```csharp
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs();
genocs.Build();

var app = builder.Build();
app.UseGenocs();
```

### 2. In-Memory CQRS Registration and Dispatch

Genocs.Core provides assembly-scanned handler registration and in-process dispatchers for commands, queries, and events.

#### Builder-Based Registration

- **`AddCommandHandlers()`**: Registers all loaded `ICommandHandler<TCommand>` implementations as transient services.
- **`AddQueryHandlers()`**: Registers all loaded `IQueryHandler<TQuery, TResult>` implementations as transient services.
- **`AddEventHandlers()`**: Registers all loaded `IEventHandler<TEvent>` implementations as transient services.
- **`AddInMemoryCommandDispatcher()`**: Registers `ICommandDispatcher`.
- **`AddInMemoryQueryDispatcher()`**: Registers `IQueryDispatcher`.
- **`AddInMemoryEventDispatcher()`**: Registers `IEventDispatcher`.

#### ServiceCollection-Based Registration

- **`AddHandlers(string project)`**: Scans loaded assemblies whose names contain the supplied project string and registers command, query, and event handlers as scoped services.
- **`AddDispatchers()`**: Registers `IDispatcher`, `ICommandDispatcher`, `IQueryDispatcher`, and `IEventDispatcher` as singletons.

#### Dispatcher Implementations

- **`CommandDispatcher`**: Creates an async scope and resolves exactly one command handler.
- **`QueryDispatcher`**: Supports both generic and runtime-resolved query execution.
- **`EventDispatcher`**: Resolves all matching event handlers and awaits them with `Task.WhenAll`.
- **`InMemoryDispatcher`**: Unified facade over command, query, and event dispatching.

**Purpose:**
This CQRS layer is designed for in-process orchestration. It gives applications a consistent dispatch boundary before they adopt external messaging, API endpoint adapters, or other runtime modules.

### 3. Domain Entities, Aggregates, and Domain Events

Genocs.Core supplies concrete base classes that implement the domain contracts declared in `Genocs.Common`.

- **`Entity` / `Entity<TPrimaryKey>`**: Base entity types with identity storage, transient detection, equality semantics, and string formatting.
- **`AggregateRoot` / `AggregateRoot<TPrimaryKey>`**: Aggregate root base classes with a `List<IEvent>? DomainEvents` collection.
- **`DomainEvent`**: Base class for framework-level domain events with a `TriggeredOn` timestamp.

**Entity Lifecycle Event Types:**
- **`EntityCreatedEvent<TEntity>`** and `EntityCreatedEvent.WithEntity(entity)`
- **`EntityUpdatedEvent<TEntity>`** and `EntityUpdatedEvent.WithEntity(entity)`
- **`EntityDeletedEvent<TEntity>`** and `EntityDeletedEvent.WithEntity(entity)`

**Key Features:**
- Default-ID shortcut types based on `DefaultIdType`
- Identity-based equality with transient entity safeguards
- Aggregate-local domain event storage
- Simple event factory methods for entity lifecycle notifications

### 4. Repository and Persistence Foundations

The package includes two repository layers: Ardalis.Specification-aligned aggregate repositories and a legacy-style generic repository base for entity persistence workflows.

- **`IRepository<T>`**: Read/write repository contract for aggregate roots based on `Ardalis.Specification.IRepositoryBase<T>`.
- **`IReadRepository<T>`**: Read-only repository contract for aggregate roots.
- **`IRepositoryWithEvents<T>`**: Marker contract for repositories that add entity lifecycle events to aggregates.
- **`RepositoryBase<TEntity, TKey>`**: Abstract repository base exposing `GetAll`, lookup, insert, update, delete, and count helpers for `Genocs.Common.Domain.Repositories.IRepository<TEntity, TKey>` implementations.
- **`IDapperRepository`**: Raw SQL query abstraction for `QueryAsync`, `QueryFirstOrDefaultAsync`, and `QuerySingleAsync`.

**Mapping and Convention Support:**
- **`TableMappingAttribute`**: Associates a class or struct with a table or collection name.
- **`AutoRepositoryTypesAttribute`**: Declares repository interface and implementation pairs for convention-driven registration scenarios.

**Capabilities:**
- Aggregate-focused repository contracts for new infrastructure packages
- Reusable CRUD and count behavior for custom repository implementations
- Specification-compatible repository surface through Ardalis.Specification
- Optional raw SQL reads via Dapper-style abstractions

Core repository runtime helpers intentionally exclude inline multi-tenancy branches and legacy infrastructure placeholders. Multi-tenancy policy belongs in dedicated companion packages so repository behavior stays deterministic and provider-agnostic.

### 5. Auditing and Entity Metadata Helpers

Genocs.Core includes audited entity hierarchies and helper utilities for populating creation, modification, and deletion metadata.

- **`CreationAuditedEntity` / `CreationAuditedAggregateRoot`**: Track `CreatedAt` and creator identity.
- **`AuditedEntity` / `AuditedAggregateRoot`**: Add modification metadata such as `LastUpdate` and `UpdatedBy`.
- **`FullAuditedEntity` / `FullAuditedAggregateRoot`**: Add soft-delete metadata including `IsDeleted`, `DeletedAt`, and `DeletedBy`.
- **`EntityAuditingHelper`**: Applies creation and modification audit values to compatible entity interfaces.
- **`Trail`, `TrailType`, `AuditDto`, `GetMyAuditLogsRequest`**: Types for working with audit trail records.
- **`IAuditService`**: Contract for querying audit history.

**Features:**
- Ready-made audited base classes for domain models
- Soft-delete-aware entity bases
- Central helper methods for setting audit properties consistently
- Audit trail contracts that higher-level packages can implement

Auditing helpers keep cross-cutting behavior focused on audit metadata assignment. Tenant resolution and tenancy policy should be composed by infrastructure packages that implement multi-tenancy concerns explicitly.

### 6. Utility Extensions and Helper APIs

Genocs.Core contains reusable helpers for common runtime tasks.

- **Collection extensions**: `IsNullOrEmpty`, `AddIfNotContains`, dictionary `GetOrDefault`, and dictionary `GetOrAdd`.
- **String extensions**: Prefix and postfix enforcement, substring helpers, line-ending normalization, occurrence indexing, hashing, and formatting utilities.
- **Object and exception extensions**: Convenience helpers for casting and exception inspection.
- **`Encryption`**: RSA XML import and export helpers through `FromXmlFile(...)` and `ToXmlFile(...)`.

Legacy `<RSAKeyValue>` XML key support remains available for backward compatibility. New integrations should prefer modern key material formats and storage patterns, while XML import paths are kept with strict validation and explicit failure types.

**Operational Value:**
These helpers reduce duplicated plumbing across Genocs packages and host applications, especially in startup, diagnostics, and infrastructure-adjacent code.

### 7. Exception and Failure Model

The package defines a small framework-specific exception hierarchy.

- **`GenocsException`**: Base exception for Genocs-specific runtime failures.
- **`GenocsException.InvalidConfigurationException`**: Nested configuration-specific exception type.
- **`EntityNotFoundException`**: Entity lookup failure exception carrying entity type and identifier details.

**Benefits:**
- Distinguishes framework failures from business exceptions
- Standardizes missing-entity behavior in repository workflows
- Preserves entity type and identifier context for logging and diagnostics

### 8. Startup Endpoints, Health, and Service Identity

`AddGenocs(...)` and the builder extensions configure baseline runtime services that many Genocs hosts use immediately.

- **Health checks**: Registers health checks and a default `self` liveness check tagged as `live`.
- **`MapDefaultEndpoints(this WebApplication)`**: In development only, maps `/`, `/healthz`, and `/alive` as anonymous endpoints.
- **`MapDefaultEndpoints(this IApplicationBuilder)`**: Maps the same endpoints through endpoint routing without the development guard.
- **`IServiceId` / `ServiceId`**: Registers a singleton per-process service identity.
- **`AppOptions` integration**: Reads the `app` configuration section for service name, version, and banner settings.

**Why It Matters:**
- New services get liveness and readiness endpoints quickly
- Host identity is available early for diagnostics and distributed tracing
- Service branding and version output can be controlled from configuration

## Architecture Integration

### Bounded Contexts

The library supports bounded context implementation through:
- Aggregate root and entity base classes that keep model boundaries explicit
- Repository contracts that can be implemented per persistence technology
- Domain event collection at aggregate level
- Auditing bases that can be selectively applied to specific domain models

### Microservices

Designed for microservices architectures:
- Builder-driven startup setup keeps service composition consistent across hosts
- In-memory CQRS dispatching works well inside a single service boundary
- Health endpoints and service identity simplify container orchestration and diagnostics
- Infrastructure packages can layer on top of the same core abstractions

### Clean Architecture

Supports clean architecture principles:
- Domain models can depend on shared contracts and concrete runtime primitives without taking dependencies on transport or storage adapters
- Host composition stays in the application boundary through `AddGenocs(...)` and related extensions
- CQRS separates write, read, and event flows clearly
- Repository contracts and audited entities can be implemented or extended in infrastructure packages

## Design Patterns Supported

1. **Builder Pattern**: `IGenocsBuilder` and `GenocsBuilder` coordinate host composition.
2. **Repository Pattern**: Aggregate contracts and `RepositoryBase<TEntity, TKey>` support persistence abstractions.
3. **Command Pattern**: Commands are dispatched through `ICommandDispatcher` and handled by registered handlers.
4. **Query Pattern**: Queries are executed through `IQueryDispatcher` and typed handlers.
5. **Observer Pattern**: Events fan out to multiple `IEventHandler<TEvent>` implementations.
6. **Initializer Pattern**: `IInitializer` and `IStartupInitializer` define startup workflows.
7. **Specification Pattern**: Aggregate repositories align with Ardalis.Specification.
8. **Template Method Pattern**: `RepositoryBase<TEntity, TKey>` leaves storage-specific operations abstract while implementing shared behavior.

## Best Practices

### Host Composition

- Call `IGenocsBuilder.Build()` before `builder.Build()` so queued build actions run.
- Use `AddInitializer(IInitializer)` when you already have an initializer instance, and `AddInitializer<TInitializer>()` only after registering `TInitializer` in DI.
- Treat `AddGenocs(...)` as the first runtime module in your host setup so later packages can build on the registered services.

### CQRS Registration

- Use builder-based handler registration when all relevant assemblies are already loaded into the AppDomain.
- Use `AddHandlers("ProjectName")` when you want narrower scanning based on assembly name matching.
- Keep handlers stateless because builder-based scanning registers them as transient services.

### Domain and Repositories

- Use `AggregateRoot` only for true consistency boundaries that own domain events.
- Reserve `IRepository<T>` and `IReadRepository<T>` for aggregate persistence, not arbitrary projections.
- Derive from `RepositoryBase<TEntity, TKey>` only when you are implementing a custom repository layer and need the shared CRUD semantics.

### Health and Configuration

- Provide the `app` section if you want startup banners and friendly service names.
- Be deliberate about `MapDefaultEndpoints(...)` in production because the `IApplicationBuilder` overload does not restrict itself to development.
- Register additional health checks alongside the default liveness check when external dependencies should affect readiness.
- Keep `Genocs.Core` warning-clean. Shared build settings treat warnings as errors for the `Genocs.Core` project so compiler, nullability, and analyzer regressions fail fast during local builds and CI.

## Usage Scenarios

### Bootstrap a New Genocs Service

- Create a single `IGenocsBuilder` through `AddGenocs(...)`
- Add core runtime modules and build actions before building the host
- Run startup initializers with `UseGenocs()` after `builder.Build()`

### Add In-Process CQRS to a Modular Application

- Register command, query, and event handlers by scanning loaded assemblies
- Resolve dispatchers through DI instead of calling handlers directly
- Keep application services independent from transport concerns

### Model Domain Aggregates With Auditing

- Inherit from `AggregateRoot` and one of the audited base classes where appropriate
- Store lifecycle events in `DomainEvents`
- Apply auditing helpers or infrastructure code to populate audit fields consistently

### Build Custom Persistence Adapters

- Implement `IRepository<T>` or `IReadRepository<T>` for aggregate-based infrastructure
- Extend `RepositoryBase<TEntity, TKey>` for custom entity repository implementations
- Use `IDapperRepository` where raw SQL read models are preferable

## Dependencies

Genocs.Core depends on a small set of runtime packages and one framework reference.

- **Genocs.Common**
- **Spectre.Console**
- **Ardalis.Specification**
- **MediatR.Contracts**
- **Scrutor**
- **Microsoft.AspNetCore.App**

## Installation

```bash
dotnet add package Genocs.Core
```

## Related Libraries

- **Genocs.Common**: Provides the contracts and shared abstractions implemented by Genocs.Core.
- **Genocs.WebApi**: Builds HTTP endpoint composition on top of the core runtime setup.
- **Genocs.Persistence.MongoDB**: Implements repository and persistence workflows for MongoDB.
- **Genocs.Persistence.EFCore**: Implements repository and persistence workflows for Entity Framework Core.

## Support and Documentation

- **Documentation**: [https://learn.fiscanner.net/](https://learn.fiscanner.net/)
- **Source Code**: [https://github.com/Genocs/genocs-library](https://github.com/Genocs/genocs-library)
- **Issues**: [https://github.com/Genocs/genocs-library/issues](https://github.com/Genocs/genocs-library/issues)
- **Changelog**: [https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md](https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md)

## License

This library is released under the MIT License. See [LICENSE](https://github.com/Genocs/genocs-library/blob/main/LICENSE) file for details.

## Contributing

Contributions are welcome! Please read the [Code of Conduct](https://github.com/Genocs/genocs-library/blob/main/CODE_OF_CONDUCT.md) before submitting pull requests.

## Author

**Giovanni Emanuele Nocco**

Creator and maintainer of the Genocs ecosystem, focused on reusable .NET building blocks for microservices and enterprise application development.
