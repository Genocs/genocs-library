# Genocs.Common Library

## Overview

**Genocs.Common** is the contract-first foundation package for the Genocs ecosystem. It defines the shared abstractions that other Genocs packages implement: domain entity contracts, repository interfaces, CQRS message and handler contracts, paging models, notifications, startup markers, metadata attributes, and a small set of utility types. Use this package when you want application and domain layers to depend on stable contracts without taking a dependency on a specific infrastructure or runtime implementation.

[![NuGet](https://img.shields.io/nuget/v/Genocs.Common.svg)](https://www.nuget.org/packages/Genocs.Common/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Common.svg)](https://www.nuget.org/packages/Genocs.Common/)

## Target Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0

## Key Design Principles

The library is designed with the following principles in mind:

- **Contract-First Design**: Keep domain and application code dependent on abstractions that can be implemented by other Genocs packages or custom infrastructure.
- **Implementation Neutrality**: Avoid embedding a persistence engine, transport, or web framework into the core contracts.
- **DDD-Friendly Modeling**: Provide the minimum interfaces needed for entities, aggregates, repositories, and auditing concerns.
- **CQRS Alignment**: Separate commands, queries, and events with explicit dispatch and handler contracts.
- **Composable Conventions**: Use marker interfaces and attributes to support assembly scanning, startup workflows, and metadata-driven behaviors.

## Core Components

### 1. Domain Modeling Contracts


Genocs.Common provides the core interfaces used to model entities and aggregate roots in a DDD-style domain.

- **`IIdentifiable<TKey>`**: Minimal typed identity contract reused by entity abstractions.
- **`IEntity`**: Base entity contract with `IsTransient()` for persistence-state checks.
- **`IEntity<TKey>`**: Typed entity contract that composes `IIdentifiable<TKey>`.
- **`IAggregateRoot`**: Marker contract for aggregate roots.
- **`IAggregateRoot<TKey>`**: Aggregate root with typed identity and domain event support.
- **`IGeneratesDomainEvents`**: Exposes a `List<IEvent>? DomainEvents` collection for aggregate-level event tracking.
- **`IRepositoryOfEntity<TEntity, TKey>`**: Async-first repository contract for CRUD and queries. All methods are asynchronous and accept a `CancellationToken`. Retrieval methods such as `GetByIdAsync` return `null` if not found, making not-found semantics explicit. Synchronous methods have been removed for safety and modern .NET compatibility.

**Key Features:**
- Explicit aggregate root boundaries
- Typed identities through `IEntity<TKey>`
- Domain event collection on aggregates

**Example Use Cases:**
```csharp
using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Entities.Auditing;

public sealed class Order : IAggregateRoot<Guid>, IFullAudited
{
    public Guid Id { get; init; }
    public List<IEvent>? DomainEvents { get; } = [];

    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public Guid CreatorUserId { get; set; }
    public DateTime? LastUpdate { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public bool IsTransient() => Id == Guid.Empty;
}
```

### 2. Repository and Persistence Contracts

The package defines persistence-facing abstractions without choosing a storage technology.

#### Repository Access

- **`IRepository<TEntity, TKey>`**: Marker interface for repository registration by convention.
- **`IRepositoryOfEntity<TEntity, TKey>`**: Full repository contract with query, lookup, insert, update, and delete operations.

#### Unit of Work and Loading

- **`IUnitOfWork`**: Saves pending changes through `Task<int> Save()`.
- **`ISupportsExplicitLoading<TEntity, TPrimaryKey>`**: Explicitly loads navigation properties and collections when the underlying implementation supports it.

#### Database Initialization

- **`IDatabaseInitializer`**: Initializes backing databases asynchronously.
- **`ICustomSeeder`**: Seeds custom data during startup or provisioning.

#### Connection String Contracts

- **`IConnectionStringValidator`**: Validates provider-specific connection strings.
- **`IConnectionStringSecurer`**: Removes or masks sensitive data before logging or display.

**Purpose:**
These contracts allow the domain and application layers to define data-access expectations once, then bind them to MongoDB, EF Core, or custom implementations elsewhere in the Genocs stack.

### 3. CQRS Messaging Contracts

Genocs.Common separates write, read, and event flows through lightweight CQRS contracts.

- **`ICommand`**: Marker interface for state-changing messages.
- **`ICommandHandler<TCommand>`**: Async command handler contract.
- **`ICommandDispatcher`**: Sends commands for execution.

**Key Features:**
- Explicit command dispatch boundary
- Async handler contracts
- Legacy handler compatibility through `ICommandHandlerLegacy<T>`

### 4. Query, Paging, and Result Models

The query namespace contains contracts and helper models for paged read workflows.

- **`IQuery` / `IQuery<T>`**: Marker interfaces for queries.
- **`IQueryHandler<TQuery, TResult>`**: Async query handler contract.
- **`IQueryDispatcher`**: Executes typed or generic queries.

**Capabilities:**
- `IPagedQuery` for page, size, sort, and order metadata
- `PagedQueryBase` and `PagedQueryWithFilter` for reusable request models
- `PagedResultBase` and `PagedResult<T>` for consistent paged responses

### 5. Events, Notifications, and Application Services

This package defines contracts for system events, UI or client notifications, and a few common app-level services.

- **`IEvent` / `IEventHandler<TEvent>` / `IEventDispatcher`**: Core event publishing contracts.
- **`IRejectedEvent` / `RejectedEvent`**: Standardized rejection event shape with `Reason` and `Code`.
- **`INotificationMessage` / `INotificationSender`**: Notification transport abstractions for broadcast, group, and user delivery. **Note:** As of March 2026, this interface no longer inherits a DI lifetime marker. Lifetime is now an infrastructure concern.
- **`ICurrentUser`**: Abstraction for authenticated user context, claims, tenant, and role checks.
- **`IJobService`**: Contract for enqueueing, scheduling, deleting, and requeueing background jobs. **Note:** As of March 2026, this interface no longer inherits a DI lifetime marker. Lifetime is now an infrastructure concern.
- **`ISerializerService`**: Serialization and deserialization abstraction. **Note:** As of March 2026, this interface no longer inherits a DI lifetime marker. Lifetime is now an infrastructure concern.
- **`IDto`**: Marker interface for DTO types.

**Features:**
- `BasicNotification` with severity labels
- `JobNotification` for job progress updates
- `StatsChangedNotification` as a marker notification message

### 6. Service Lifetimes and Startup Conventions

Genocs.Common now uses a canonical set of DI marker interfaces in the `Genocs.Common.Dependency` namespace:

- **`ISingletonDependency`**: Marker for singleton registration by scanning (canonical).
- **`ITransientDependency`**: Marker for transient registration by scanning (canonical).
- **`IScopedDependency`**: Marker for scoped registration (canonical, new).
- **`IInitializer`**: Async initialization contract for boot tasks.
- **`IStartupInitializer`**: Aggregates multiple `IInitializer` instances into a startup pipeline.

**Obsolete:**
- `IScopedService` and `ITransientService` (in `Genocs.Common.Interfaces`) are now obsolete and inherit from the canonical markers. Use only the canonical marker interfaces for new code.

**Benefits:**
- Consistent lifetime conventions across packages
- Cleaner startup orchestration
- Reduced boilerplate in host projects

### 7. Metadata, Type Utilities, and Serialization Helpers

The library exposes metadata attributes and generic type helpers used by higher-level packages.

- **`MessageAttribute`**: Annotates messages with exchange, topic, queue, queue type, error queue, and subscription metadata.
- **`DecoratorAttribute`**: Marks decorators for scanner-aware registration strategies.
- **`HiddenAttribute`**: Marks properties that should be hidden from reflection-driven consumers.
- **`PublicContractAttribute`**: Marks classes as public contracts.
- **`Extensions`**: Reflection-based helpers for creating default object graphs and default property values.

**Benefits:**
- Transport metadata stays close to message contracts
- Assembly scanners can opt in or opt out of types cleanly
- Shared conventions remain discoverable in code

### 8. Configuration, Identity, and Miscellaneous Utility Types

This package also includes a small set of reusable primitives for configuration and runtime identity.

- **`AppOptions`**: Binds the `app` configuration section with service name, instance, version, and display flags.
- **`IServiceId` / `ServiceId`**: Provides a per-instance GUID-based service identifier.
- **`ITypeList` / `TypeList`**: Stores and validates types constrained to a base type.

## Architecture Integration

### Bounded Contexts

The library supports bounded context implementation through:
- Aggregate root and entity contracts that keep domain boundaries explicit
- Repository interfaces that can be implemented per context
- Domain events collected at aggregate boundaries
- Context-specific auditing and soft-delete contracts

### Microservices

Designed for microservices architectures:
- Shared contracts can be reused across service boundaries without infrastructure coupling
- Message metadata can live on integration contracts through `MessageAttribute`
- Notifications and service identity abstractions support distributed runtime concerns
- Startup and lifetime markers keep service composition consistent across hosts

### Clean Architecture

Supports clean architecture principles:
- Domain and application layers depend on interfaces rather than adapters
- Infrastructure implementations can live in separate packages
- Cross-cutting concerns remain standardized without forcing a runtime dependency
- CQRS contracts keep read and write paths explicit

## Design Patterns Supported

1. **Repository Pattern**: `IRepositoryOfEntity<TEntity, TKey>` defines the domain-facing repository surface.
2. **Unit of Work Pattern**: `IUnitOfWork` provides a persistence commit boundary.
3. **Command Pattern**: `ICommand` and `ICommandHandler<TCommand>` model state-changing operations.
4. **Query Pattern**: `IQuery<T>` and `IQueryHandler<TQuery, TResult>` model read-only requests.
5. **Observer Pattern**: `IEvent` and `IEventHandler<TEvent>` enable publish-subscribe workflows.
6. **Decorator Pattern**: `DecoratorAttribute` supports decorator-aware scanning and registration.
7. **Marker Interface Pattern**: Lifetime and DTO interfaces communicate behavior through conventions.
8. **Initializer Pattern**: `IInitializer` and `IStartupInitializer` standardize boot-time tasks.

## Best Practices

### Domain Contracts

- Use `IAggregateRoot<TKey>` only for true transaction boundaries.
- Keep domain events on aggregates, not on every entity type.
- Apply auditing interfaces only where the extra metadata is required.

### Repository Design

- Expose repositories from domain or application layers, then implement them in infrastructure packages.
- Prefer `IRepositoryOfEntity<TEntity, TKey>` for aggregate persistence, not for arbitrary read models.
- Use `ISupportsExplicitLoading<TEntity, TPrimaryKey>` only when your implementation genuinely supports it.

### CQRS Usage

- Keep commands focused on state changes and queries focused on data retrieval.
- Return `PagedResult<T>` for pageable endpoints to keep result contracts consistent.
- Use `RejectedEvent` only for integration-style failure signaling, not as a substitute for domain validation.

### Conventions and Metadata

- Use marker interfaces consistently across a solution so scanning rules stay predictable.
- Put `MessageAttribute` on public message contracts, not on implementation details.
- Use `TypeList` when you need assembly-discovered types constrained to a known base type.

## Usage Scenarios

### Shared Domain Contracts Across Services

- Define entities, repositories, and auditing expectations once
- Keep service implementations free to choose their own infrastructure adapters
- Reuse the same domain contracts in tests and production hosts

### CQRS-Centered Application Layers

- Model commands, queries, and events with stable contracts
- Keep dispatching concerns abstract until runtime composition
- Standardize paging and result shapes for read models

### Infrastructure-Agnostic Notifications

- Define notification payloads independently from SignalR or broker implementations
- Route notifications to groups or users through `INotificationSender`
- Reuse the same notification contracts in Web API and worker solutions

### Startup and Hosting Conventions

- Centralize boot tasks behind `IInitializer`
- Use lifetime markers for consistent DI registration by convention
- Assign a stable per-instance `ServiceId` for runtime diagnostics

## Dependencies

Genocs.Common keeps its consumer-facing package surface intentionally minimal.

- **No direct runtime NuGet dependencies**
- **.NET base class libraries only**
- **Repository-wide analyzers are development-time only and not part of the package runtime contract**

## Installation

```bash
dotnet add package Genocs.Common
```

## Related Libraries

- **Genocs.Core**: Provides runtime implementations for many Genocs.Common contracts.
- **Genocs.WebApi.CQRS**: Adapts Genocs CQRS contracts to Web API endpoint workflows.
- **Genocs.Persistence.MongoDB**: Implements repository and persistence contracts for MongoDB.
- **Genocs.Persistence.EFCore**: Implements repository and persistence contracts for Entity Framework Core.

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
