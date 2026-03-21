# Genocs.Common Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Common is responsible for
- Which abstractions to implement for specific goals
- Where source of truth lives for each primitive
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Common |
| Project file | [src/Genocs.Common/Genocs.Common.csproj](src/Genocs.Common/Genocs.Common.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | Shared primitives, CQRS contracts, domain model abstractions, and cross-cutting utility types for all Genocs packages |
| Core themes | CQRS interfaces, domain-driven design building blocks, paged query support, notification model, DI markers, persistence initialization |

## Use This Package When

- Defining commands, queries, or events that flow through the Genocs pipeline
- Modeling domain entities and aggregate roots with a primary key
- Implementing a repository abstraction supporting LINQ queries and async operations
- Building paged query responses consumed by WebApi endpoints
- Marking services for DI lifetime conventions (`ISingletonDependency`, `ITransientDependency`)
- Wiring up database seeders and initializers via `IDatabaseInitializer`
- Publishing or handling in-app notifications (`INotificationMessage`, `BasicNotification`)

## Do Not Assume

- This package has no runtime dependencies — it is pure abstractions and lightweight primitives; no framework services are registered automatically
- `DefaultIdType` is a global alias for `System.Guid` defined in `Directory.Build.props`; do not use raw `Guid` in entity primary keys
- `ICommand`, `IQuery`, `IEvent` carry no data by themselves; concrete command/query classes implement these interfaces and add their own properties

## High-Value Entry Points

### CQRS Contracts

- `ICommand` in [src/Genocs.Common/CQRS/Commands/ICommand.cs](src/Genocs.Common/CQRS/Commands/ICommand.cs)
- `ICommandHandler<TCommand>` in [src/Genocs.Common/CQRS/Commands/ICommandHandler.cs](src/Genocs.Common/CQRS/Commands/ICommandHandler.cs)
- `ICommandDispatcher` in [src/Genocs.Common/CQRS/Commands/ICommandDispatcher.cs](src/Genocs.Common/CQRS/Commands/ICommandDispatcher.cs)
- `IEvent` in [src/Genocs.Common/CQRS/Events/IEvent.cs](src/Genocs.Common/CQRS/Events/IEvent.cs)
- `IEventHandler<TEvent>` in [src/Genocs.Common/CQRS/Events/IEventHandler.cs](src/Genocs.Common/CQRS/Events/IEventHandler.cs)
- `IEventDispatcher` in [src/Genocs.Common/CQRS/Events/IEventDispatcher.cs](src/Genocs.Common/CQRS/Events/IEventDispatcher.cs)
- `IRejectedEvent` / `RejectedEvent` in [src/Genocs.Common/CQRS/Events/IRejectedEvent.cs](src/Genocs.Common/CQRS/Events/IRejectedEvent.cs)
- `IDispatcher` in [src/Genocs.Common/CQRS/Commons/IDispatcher.cs](src/Genocs.Common/CQRS/Commons/IDispatcher.cs)

### Query & Paging Contracts

- `IQuery` in [src/Genocs.Common/CQRS/Queries/IQuery.cs](src/Genocs.Common/CQRS/Queries/IQuery.cs)
- `IQueryHandler<TQuery, TResult>` in [src/Genocs.Common/CQRS/Queries/IQueryHandler.cs](src/Genocs.Common/CQRS/Queries/IQueryHandler.cs)
- `IQueryDispatcher` in [src/Genocs.Common/CQRS/Queries/IQueryDispatcher.cs](src/Genocs.Common/CQRS/Queries/IQueryDispatcher.cs)
- `IPagedQuery` in [src/Genocs.Common/CQRS/Queries/IPagedQuery.cs](src/Genocs.Common/CQRS/Queries/IPagedQuery.cs)
- `PagedQueryBase` in [src/Genocs.Common/CQRS/Queries/PagedQueryBase.cs](src/Genocs.Common/CQRS/Queries/PagedQueryBase.cs)
- `PagedResult<T>` in [src/Genocs.Common/CQRS/Queries/PagedResult.cs](src/Genocs.Common/CQRS/Queries/PagedResult.cs)
- `PagedResultBase` in [src/Genocs.Common/CQRS/Queries/PagedResultBase.cs](src/Genocs.Common/CQRS/Queries/PagedResultBase.cs)
- `IPagedFilter` / `PagedQueryWithFilter` in [src/Genocs.Common/CQRS/Queries/IPagedFilter.cs](src/Genocs.Common/CQRS/Queries/IPagedFilter.cs)
- `ISearchRequest` / `SearchRequest` in [src/Genocs.Common/CQRS/Queries/ISearchRequest.cs](src/Genocs.Common/CQRS/Queries/ISearchRequest.cs)

### Domain Model Abstractions

- `IEntity` / `IEntity<TKey>` in [src/Genocs.Common/Domain/Entities/IEntity.cs](src/Genocs.Common/Domain/Entities/IEntity.cs)
- `IEntityOfTPrimaryKey` in [src/Genocs.Common/Domain/Entities/IEntityOfTPrimaryKey.cs](src/Genocs.Common/Domain/Entities/IEntityOfTPrimaryKey.cs)
- `IAggregateRoot` / `IAggregateRoot<TKey>` / `IGeneratesDomainEvents` in [src/Genocs.Common/Domain/Entities/IAggregateRoot.cs](src/Genocs.Common/Domain/Entities/IAggregateRoot.cs)
- `ISoftDelete` in [src/Genocs.Common/Domain/Entities/ISoftDelete.cs](src/Genocs.Common/Domain/Entities/ISoftDelete.cs)
- Auditing interfaces in [src/Genocs.Common/Domain/Entities/Auditing/](src/Genocs.Common/Domain/Entities/Auditing/)

### Repository & Persistence Abstractions

- `IRepository<TEntity, TKey>` / `IRepositoryOfEntity<TEntity, TKey>` in [src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs](src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs)
- `IUnitOfWork` in [src/Genocs.Common/Domain/Repositories/IUnitOfWork.cs](src/Genocs.Common/Domain/Repositories/IUnitOfWork.cs)
- `ISupportsExplicitLoading` in [src/Genocs.Common/Domain/Repositories/ISupportsExplicitLoading.cs](src/Genocs.Common/Domain/Repositories/ISupportsExplicitLoading.cs)
- `IDatabaseInitializer` in [src/Genocs.Common/Persistence/Initialization/IDatabaseInitializer.cs](src/Genocs.Common/Persistence/Initialization/IDatabaseInitializer.cs)
- `ICustomSeeder` in [src/Genocs.Common/Persistence/Initialization/ICustomSeeder.cs](src/Genocs.Common/Persistence/Initialization/ICustomSeeder.cs)

### Cross-Cutting Service Interfaces

- `ICurrentUser` in [src/Genocs.Common/Interfaces/ICurrentUser.cs](src/Genocs.Common/Interfaces/ICurrentUser.cs)
- `IDto` in [src/Genocs.Common/Interfaces/IDto.cs](src/Genocs.Common/Interfaces/IDto.cs)
- `ISerializerService` in [src/Genocs.Common/Interfaces/ISerializerService.cs](src/Genocs.Common/Interfaces/ISerializerService.cs)
- `INotificationSender` in [src/Genocs.Common/Interfaces/INotificationSender.cs](src/Genocs.Common/Interfaces/INotificationSender.cs)
- `IJobService` in [src/Genocs.Common/Interfaces/IJobService.cs](src/Genocs.Common/Interfaces/IJobService.cs)
- `IScopedService` / `ITransientService` in [src/Genocs.Common/Interfaces/IScopedService.cs](src/Genocs.Common/Interfaces/IScopedService.cs)

### Type Attributes & Utilities

- `MessageAttribute` in [src/Genocs.Common/Types/MessageAttribute.cs](src/Genocs.Common/Types/MessageAttribute.cs)
- `IIdentifiable<T>` in [src/Genocs.Common/Types/IIdentifiable.cs](src/Genocs.Common/Types/IIdentifiable.cs)
- `DecoratorAttribute` / `HiddenAttribute` / `PublicContractAttribute` in [src/Genocs.Common/Types/DecoratorAttribute.cs](src/Genocs.Common/Types/DecoratorAttribute.cs)
- `IInitializer` / `IStartupInitializer` in [src/Genocs.Common/Types/IInitializer.cs](src/Genocs.Common/Types/IInitializer.cs)
- `Extensions.GetDefaultInstance(this Type)` in [src/Genocs.Common/Types/Extensions.cs](src/Genocs.Common/Types/Extensions.cs)
- `Extensions.SetDefaultInstanceProperties(this object)` in [src/Genocs.Common/Types/Extensions.cs](src/Genocs.Common/Types/Extensions.cs)

### Notifications & Configuration

- `BasicNotification` / `INotificationMessage` in [src/Genocs.Common/Notifications/BasicNotification.cs](src/Genocs.Common/Notifications/BasicNotification.cs)
- `JobNotification` / `StatsChangedNotification` in [src/Genocs.Common/Notifications/](src/Genocs.Common/Notifications/)
- `AppOptions` (config key: `app`) in [src/Genocs.Common/Configurations/AppOptions.cs](src/Genocs.Common/Configurations/AppOptions.cs)
- `ISingletonDependency` / `ITransientDependency` in [src/Genocs.Common/Dependency/](src/Genocs.Common/Dependency/)
- `ITypeList<T>` / `TypeList<T>` in [src/Genocs.Common/Collections/ITypeList.cs](src/Genocs.Common/Collections/ITypeList.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Define a command | Implement `ICommand` | Add your data properties on the concrete class |
| Define a query returning a page | Implement `IPagedQuery`; return `PagedResult<T>` | Use `PagedQueryBase` for base implementation with `Page`, `Results`, `OrderBy`, `SortOrder` |
| Define an event | Implement `IEvent` | For rejected/failure events, also implement `IRejectedEvent` |
| Define a domain entity | Implement `IEntity<TKey>` using `DefaultIdType` as `TKey` | Use `IAggregateRoot<TKey>` for roots that generate domain events |
| Implement a repository | Extend `IRepositoryOfEntity<TEntity, TKey>` | Full LINQ + async query surface |
| Map a message to a queue | Apply `[Message(exchange, queue)]` to command/event class | Used by `Genocs.Messaging.RabbitMQ` to resolve routing |
| Tag a class as decorator | Apply `[Decorator]` attribute | Scrutor-based decorator discovery uses this marker |
| Access the current user | Resolve `ICurrentUser` | Exposes `GetUserId()`, `GetUserEmail()`, `GetTenant()`, `IsInRole()`, `GetUserClaims()` |
| Seed a database at startup | Implement `ICustomSeeder` and `IDatabaseInitializer` | Wire via DI; call `InitializeDatabasesAsync()` in hosted startup |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Common
```

### Usage Example

Genocs.Common contains only interfaces and primitives — no `Program.cs` wiring is required. Usage is purely by implementing the provided interfaces:

```csharp
using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;
using Genocs.Common.Domain.Entities;
using Genocs.Common.Types;

// Define a command
public record CreateOrderCommand(DefaultIdType CustomerId, decimal Amount) : ICommand;

// Define a command handler
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
{
    public async Task HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        // business logic
    }
}

// Define a paged query
public class GetOrdersQuery : PagedQueryBase, IPagedQuery
{
    public string? CustomerId { get; set; }
}

// Define a paged result usage
var result = PagedResult<OrderDto>.Create(items, page, pageSize, totalPages, totalCount);

// Domain entity
public class Order : IEntity<DefaultIdType>
{
    public DefaultIdType Id { get; set; }
    public bool IsTransient() => Id == DefaultIdType.Empty;
}

// Message routing attribute for RabbitMQ
[Message(exchange: "orders", queue: "orders.create")]
public record CreateOrderCommand(...) : ICommand;
```

## Behavior Notes That Affect Agent Decisions

- `DefaultIdType` is a global `using` alias for `System.Guid`, set in `Directory.Build.props`; always use `DefaultIdType` in entity primary keys to stay consistent with the codebase convention
- `PagedResult<T>.Create(...)` is the canonical factory; never construct `PagedResult<T>` via its constructor directly from external code since its constructors are `protected`
- `IAggregateRoot<TKey>` exposes `List<IEvent>? DomainEvents` for domain event collection; implementations must populate this list and clear it after publishing
- `[MessageAttribute]` on a command or event class is the sole source of messaging routing metadata consumed by `Genocs.Messaging.RabbitMQ`; ensure exchange and queue values match infrastructure configuration
- `ICurrentUser` is a cross-cutting interface — concrete implementations are provided by `Genocs.Auth` and resolved via DI; do not instantiate directly
- `IDatabaseInitializer.InitializeDatabasesAsync()` is not called automatically — the host application must invoke it at startup
- `IPagedQuery` uses **zero-based** page indexing (`Page` starts at 0)

## Source-Accurate Capability Map

### CQRS Message Contracts

- `ICommand` — marker for write-side commands
- `ICommandHandler<TCommand>` — single-method `HandleAsync(TCommand, CancellationToken)` contract
- `ICommandDispatcher` — dispatches commands to their registered handlers
- `IMessage` — root marker for commands and events (via `IDispatcher`)

Files:

- [src/Genocs.Common/CQRS/Commands/ICommand.cs](src/Genocs.Common/CQRS/Commands/ICommand.cs)
- [src/Genocs.Common/CQRS/Commands/ICommandHandler.cs](src/Genocs.Common/CQRS/Commands/ICommandHandler.cs)
- [src/Genocs.Common/CQRS/Commands/ICommandDispatcher.cs](src/Genocs.Common/CQRS/Commands/ICommandDispatcher.cs)
- [src/Genocs.Common/CQRS/Commons/IMessage.cs](src/Genocs.Common/CQRS/Commons/IMessage.cs)

### Event Contracts

- `IEvent` — marker for integration and domain events
- `IEventHandler<TEvent>` — single-method `HandleAsync(TEvent, CancellationToken)` contract
- `IRejectedEvent` / `RejectedEvent` — rejection event with `Code` and `Reason` properties
- `IEventDispatcher` — dispatches events to their registered handlers

Files:

- [src/Genocs.Common/CQRS/Events/IEvent.cs](src/Genocs.Common/CQRS/Events/IEvent.cs)
- [src/Genocs.Common/CQRS/Events/IEventHandler.cs](src/Genocs.Common/CQRS/Events/IEventHandler.cs)
- [src/Genocs.Common/CQRS/Events/IRejectedEvent.cs](src/Genocs.Common/CQRS/Events/IRejectedEvent.cs)
- [src/Genocs.Common/CQRS/Events/RejectedEvent.cs](src/Genocs.Common/CQRS/Events/RejectedEvent.cs)

### Paged Query Support

- `IPagedQuery` — `Page`, `Results`, `OrderBy`, `SortOrder` properties
- `PagedQueryBase` — base class implementing `IPagedQuery`
- `PagedQueryWithFilter` — extends `PagedQueryBase` with a filter bag
- `PagedResult<T>` — carries `Items` collection plus pagination metadata; factory: `PagedResult<T>.Create(...)` and `PagedResult<T>.From(...)`
- `SearchRequest` — text search + paging in one object

Files:

- [src/Genocs.Common/CQRS/Queries/IPagedQuery.cs](src/Genocs.Common/CQRS/Queries/IPagedQuery.cs)
- [src/Genocs.Common/CQRS/Queries/PagedQueryBase.cs](src/Genocs.Common/CQRS/Queries/PagedQueryBase.cs)
- [src/Genocs.Common/CQRS/Queries/PagedResult.cs](src/Genocs.Common/CQRS/Queries/PagedResult.cs)
- [src/Genocs.Common/CQRS/Queries/PagedResultBase.cs](src/Genocs.Common/CQRS/Queries/PagedResultBase.cs)

### Domain Model Abstractions

- `IEntity` / `IEntity<TKey>` — base entity with transient check
- `IAggregateRoot<TKey>` — extends entity with `List<IEvent>? DomainEvents` for domain event tracking
- `ISoftDelete` — `IsDeleted` / `DeletedOn` convention marker
- Auditing interfaces in `Auditing/` — created/modified tracking

Files:

- [src/Genocs.Common/Domain/Entities/IEntity.cs](src/Genocs.Common/Domain/Entities/IEntity.cs)
- [src/Genocs.Common/Domain/Entities/IAggregateRoot.cs](src/Genocs.Common/Domain/Entities/IAggregateRoot.cs)
- [src/Genocs.Common/Domain/Entities/ISoftDelete.cs](src/Genocs.Common/Domain/Entities/ISoftDelete.cs)
- [src/Genocs.Common/Domain/Entities/Auditing/](src/Genocs.Common/Domain/Entities/Auditing/)

### Repository & Persistence Contracts

- `IRepositoryOfEntity<TEntity, TKey>` — full LINQ + async CRUD surface: `GetAll()`, `GetAllList()`, `GetAllListAsync()`, `GetAllIncluding(...)`, filter overloads
- `IUnitOfWork` — transaction boundary marker
- `IDatabaseInitializer` — `InitializeDatabasesAsync()` for startup
- `ICustomSeeder` — implements custom per-table/collection seeding logic

Files:

- [src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs](src/Genocs.Common/Domain/Repositories/IRepositoryOfEntity.cs)
- [src/Genocs.Common/Domain/Repositories/IUnitOfWork.cs](src/Genocs.Common/Domain/Repositories/IUnitOfWork.cs)
- [src/Genocs.Common/Persistence/Initialization/IDatabaseInitializer.cs](src/Genocs.Common/Persistence/Initialization/IDatabaseInitializer.cs)
- [src/Genocs.Common/Persistence/Initialization/ICustomSeeder.cs](src/Genocs.Common/Persistence/Initialization/ICustomSeeder.cs)

### Cross-Cutting Interfaces

- `ICurrentUser` — `GetUserId()`, `GetUserEmail()`, `GetTenant()`, `IsAuthenticated()`, `IsInRole(role)`, `GetUserClaims()`
- `IDto` — marker for data transfer objects
- `ISerializerService` — serialize/deserialize abstraction
- `INotificationSender` — pushes `INotificationMessage` to consumers
- `ISingletonDependency` / `ITransientDependency` — DI lifetime convention markers for auto-registration

Files:

- [src/Genocs.Common/Interfaces/ICurrentUser.cs](src/Genocs.Common/Interfaces/ICurrentUser.cs)
- [src/Genocs.Common/Interfaces/INotificationSender.cs](src/Genocs.Common/Interfaces/INotificationSender.cs)
- [src/Genocs.Common/Interfaces/ISerializerService.cs](src/Genocs.Common/Interfaces/ISerializerService.cs)
- [src/Genocs.Common/Dependency/ISingletonDependency.cs](src/Genocs.Common/Dependency/ISingletonDependency.cs)

### Type Attributes & Utilities

- `[Message(exchange, topic, queue, queueType, errorQueue, subscriptionId)]` — routing metadata for messaging infrastructure
- `[Decorator]` — marks a class as an injected decorator (used by Scrutor-based wiring)
- `[HiddenAttribute]` / `[PublicContractAttribute]` — visibility control for published contracts
- `GetDefaultInstance(this Type)` / `SetDefaultInstanceProperties(this object)` — reflection-based default value helpers

Files:

- [src/Genocs.Common/Types/MessageAttribute.cs](src/Genocs.Common/Types/MessageAttribute.cs)
- [src/Genocs.Common/Types/DecoratorAttribute.cs](src/Genocs.Common/Types/DecoratorAttribute.cs)
- [src/Genocs.Common/Types/Extensions.cs](src/Genocs.Common/Types/Extensions.cs)

## Dependencies

From [src/Genocs.Common/Genocs.Common.csproj](src/Genocs.Common/Genocs.Common.csproj):

- No external NuGet dependencies — this is a zero-dependency primitives package
- Targets: `net10.0`, `net9.0`, `net8.0`

## Related Docs

- NuGet package readme: [src/Genocs.Common/README_NUGET.md](src/Genocs.Common/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.Common-Agent-Documentation.md](docs/Genocs.Common-Agent-Documentation.md)
- Genocs.Core reference: [docs/Genocs.Core-Agent-Documentation.md](docs/Genocs.Core-Agent-Documentation.md)
