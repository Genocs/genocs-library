# Genocs.Core Library

## Overview

**Genocs.Core** is the runtime implementation layer of the Genocs ecosystem. While Genocs.Common defines contracts and abstractions, Genocs.Core provides concrete building blocks for bootstrapping services, wiring in-memory CQRS dispatching, implementing domain entities and aggregates, and enabling repository and auditing foundations for enterprise-grade .NET applications.

This package is designed for modular monoliths and microservices that need a consistent startup pipeline, DDD-friendly primitives, and framework-level utilities without locking application code into a specific infrastructure vendor.

[![NuGet](https://img.shields.io/nuget/v/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core/)

## Target Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0

## Key Design Principles

The library is designed with the following principles in mind:

- **Implementation over Abstraction**: Complements Genocs.Common by turning contracts into executable runtime behaviors
- **Convention-First Composition**: Uses assembly scanning and extension methods for low-friction setup
- **DDD and CQRS Ready**: Includes domain and dispatching primitives aligned with clean architecture patterns
- **Host-Friendly Bootstrapping**: Integrates directly with ASP.NET Core startup and DI APIs
- **Production-Oriented Defaults**: Health checks, service identity, memory caching, and startup initializers out of the box

## Core Components

### 1. Application Builder and Bootstrap Pipeline

Genocs.Core introduces a fluent runtime bootstrap layer through `IGenocsBuilder` and extension methods that scaffold service startup.

- **`IGenocsBuilder`**: Central builder abstraction exposing services, configuration, startup hooks, and build execution
- **`GenocsBuilder`**: Concrete implementation with duplicate-registration guard and deferred build actions
- **`AddGenocs(...)`**: Entry point for registering core services and base runtime configuration
- **`UseGenocs(...)`**: Executes registered startup initializers in application pipeline
- **`MapDefaultEndpoints(...)`**: Adds root endpoint and liveness/readiness health endpoints

**Key Features:**
- Unified startup composition for `WebApplicationBuilder` and `IServiceCollection`
- Deferred build actions through `AddBuildAction(...)`
- Startup initializer orchestration with `IStartupInitializer`
- Built-in health checks (`/healthz`, `/alive`) and root endpoint mapping
- Service identity registration via `IServiceId`

**Example Use Cases:**
```csharp
var genocs = builder.AddGenocs();
genocs.AddInitializer<MyInitializer>();
genocs.Build();

app.UseGenocs();
app.MapDefaultEndpoints();
```

### 2. CQRS Runtime Implementation

Genocs.Core provides in-memory dispatching and handler registration for command, query, and event workflows.

#### Commands

- **`AddCommandHandlers()`**: Registers all command handlers discovered in loaded assemblies
- **`AddInMemoryCommandDispatcher()`**: Registers `ICommandDispatcher` implementation
- **`CommandDispatcher`**: Resolves handler per command in a scoped lifetime

#### Queries

- **`AddQueryHandlers()`**: Registers query handlers via convention scanning
- **`AddInMemoryQueryDispatcher()`**: Registers `IQueryDispatcher` implementation
- **`QueryDispatcher`**: Supports both typed and generic query execution paths

#### Events

- **`AddEventHandlers()`**: Registers event handlers from loaded assemblies
- **`AddInMemoryEventDispatcher()`**: Registers `IEventDispatcher` implementation
- **`EventDispatcher`**: Publishes an event to all matching handlers asynchronously

#### Unified Dispatcher

- **`AddDispatchers()`**: Registers `IDispatcher`, `ICommandDispatcher`, `IQueryDispatcher`, and `IEventDispatcher`
- **`InMemoryDispatcher`**: Single façade for command send, query execution, and event publish

**Purpose:**
This runtime CQRS layer enables:
- In-process command/query/event orchestration
- Simple modular service composition
- Test-friendly dispatch boundaries
- Progressive adoption before introducing external message brokers

### 3. Domain-Driven Design Building Blocks

Genocs.Core contains concrete domain types that implement Genocs.Common DDD contracts.

#### Entities and Aggregates

- **`Entity` / `Entity<TKey>`**: Base entity implementations with identity, transient checks, and equality semantics
- **`AggregateRoot` / `AggregateRoot<TKey>`**: Aggregate roots with in-memory domain event collection
- **`DomainEvent`**: Base class for domain events with `TriggeredOn` timestamp

**Domain Event Wrappers:**
- `EntityCreatedEvent<T>`
- `EntityUpdatedEvent<T>`
- `EntityDeletedEvent<T>`

**Key Features:**
- Strong identity-based equality behavior
- Aggregate-level domain event accumulation
- Soft-delete helpers through entity extensions
- Typed not-found exception support (`EntityNotFoundException`)

### 4. Repository Infrastructure

Genocs.Core includes repository contracts and base implementations for common persistence workflows.

- **`IRepository<T>`**: Read/write repository contract for aggregate roots (Ardalis.Specification-based)
- **`IReadRepository<T>`**: Read-only repository contract
- **`IRepositoryWithEvents<T>`**: Repository contract intended to attach domain entity lifecycle events
- **`IDapperRepository`**: SQL-oriented read abstraction for raw query scenarios
- **`RepositoryBase<TEntity, TKey>`**: Generic repository base with CRUD, query, and count helpers

**Mapping and Convention Attributes:**
- **`AutoRepositoryTypesAttribute`**: Describes repository interface/implementation pairs
- **`TableMappingAttribute`**: Maps domain models to table/collection names

**Capabilities:**
- Shared repository base behavior
- Specification-friendly repository contracts
- Async and sync operation patterns
- Query and mutation orchestration

### 5. Auditing Infrastructure

Genocs.Core provides audited entity hierarchies and helper utilities for creation/modification/deletion metadata.

- **Creation audited types**: `CreationAuditedEntity`, `CreationAuditedAggregateRoot`
- **Modification audited types**: `AuditedEntity`, `AuditedAggregateRoot`
- **Full auditing types**: `FullAuditedEntity`, `FullAuditedAggregateRoot`
- **`EntityAuditingHelper`**: Helper methods for setting creator/modifier metadata
- **`Trail` / `TrailType`**: Audit trail model and operation type
- **`IAuditService`**: Contract for retrieving user trails

**Features:**
- Standardized created/updated/deleted metadata model
- Soft delete support integrated with audit fields
- Auditable base classes for entities and aggregates
- Lightweight audit retrieval contract for application services

### 6. Collections and Utility Extensions

Genocs.Core includes reusable extension libraries for common runtime tasks:

- **Collections extensions**: list, dictionary, collection, and enumerable helpers
- **String extensions**: casing, splitting, normalization, prefix/postfix utilities
- **Object and exception extensions**: convenience conversion and diagnostic helpers
- **Encryption helpers**: RSA XML import/export helpers for key material workflows

**Purpose:**
These utilities reduce repetitive infrastructure code and provide consistent helper behavior across packages.

### 7. Exception Model

- **`GenocsException`**: Base exception for framework-specific errors
- **`EntityNotFoundException`**: Domain-focused not-found exception for repository/entity workflows

**Benefits:**
- Consistent exception hierarchy
- Clear separation between application and framework-level faults
- Better error classification for handlers and middleware

### 8. Startup and Service Identity Utilities

- **`StartupInitializer`**: Executes registered application initializers sequentially
- **`IStartupInitializer` / `IInitializer` integration**: Controlled startup execution flow
- **`ServiceId` integration**: Registers a singleton service identity for runtime uniqueness

**Use Cases:**
- Warmup routines
- Seed initialization hooks
- One-time startup orchestration
- Service instance traceability

## Architecture Integration

### Bounded Contexts

Genocs.Core supports bounded context implementation through:
- Aggregate root foundations
- Repository abstraction and base logic
- Domain event accumulation in aggregate boundaries
- Context-specific startup and initialization routines

### Microservices

Designed for microservices and modular services:
- Simple host bootstrapping with `AddGenocs`
- In-memory CQRS dispatching per service boundary
- Health endpoints for orchestration and readiness checks
- Pluggable persistence implementations on top of repository contracts

### Clean Architecture

Supports clean architecture principles:
- Domain-centric base classes and event model
- Infrastructure-agnostic contracts from Genocs.Common, concrete runtime in Genocs.Core
- Dependency injection and startup composition at application boundary
- Clear layering between contracts, runtime orchestration, and adapters

## Design Patterns Supported

1. **Builder Pattern**: Fluent startup and service composition
2. **Repository Pattern**: Data access abstraction and reusable base logic
3. **Unit of Work-Friendly Design**: Repository base prepared for transactional orchestration
4. **Command Pattern**: Command dispatching through in-memory dispatcher
5. **Mediator-Style Dispatching**: Central `IDispatcher` for command/query/event operations
6. **Observer Pattern**: Event publishing to multiple handlers
7. **Decorator Awareness**: Handler scanning excludes decorated types through marker attributes
8. **Template Method Pattern**: Repository base methods with overridable persistence specifics

## Best Practices

### Builder and Startup

- Call `Build()` on `IGenocsBuilder` before application build finalization.
- Use startup initializers for infrastructure warmup and deterministic boot tasks.
- Map default endpoints consciously in production to align with security posture.

### CQRS Composition

- Register handlers only from relevant assemblies for faster startup and clearer boundaries.
- Keep command handlers focused on state transitions and side effects.
- Use query handlers for read-only workflows and shape-specific result models.

### Domain and Repositories

- Model aggregates as transaction boundaries and keep invariants inside aggregate methods.
- Use `RepositoryBase` as a baseline and override where persistence engine specifics are required.
- Throw `EntityNotFoundException` for absent identity-based lookups to standardize behavior.

### Auditing

- Favor audited base entities for business-critical aggregates.
- Populate creator/modifier metadata in a single application-layer strategy.
- Persist trails for regulated or high-observability domains.

## Usage Scenarios

### Service Bootstrap Standardization

- Uniform startup across multiple services
- Shared health endpoint and service identity setup
- Reusable initialization workflows

### CQRS-Driven Applications

- In-process command/query/event flow
- Fast evolution from monolith modules to service boundaries
- Consistent DI-based handler discovery

### Domain-Centric Systems

- Rich domain model implementation with entities and aggregates
- Domain event capture and later publication
- Audited entities with soft-delete support

### Data Access Foundations

- Reusable repository base for provider-specific adapters
- Hybrid repository strategy (specification + Dapper reads)
- Common exception semantics for missing entities

## Dependencies

Genocs.Core depends on:

- **Genocs.Common**
- **Spectre.Console**
- **Ardalis.Specification**
- **MediatR.Contracts**
- **Scrutor**
- **Microsoft.AspNetCore.App** (framework reference)

## Installation

```bash
dotnet add package Genocs.Core
```

## Related Libraries

- **Genocs.Common**: Contracts and abstractions used by Genocs.Core
- **Genocs.WebApi**: Web API endpoint composition on top of core runtime services
- **Genocs.Persistence.MongoDB**: MongoDB adapters for repositories and persistence
- **Genocs.Persistence.EFCore**: EF Core implementations for repository patterns
- **Genocs.Messaging**: Messaging abstractions and broker integrations for distributed workflows

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

Enterprise Architect and Software Engineer specializing in .NET, microservices, and distributed systems.
