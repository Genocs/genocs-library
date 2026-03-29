# Genocs.Common Library

## Overview

**Genocs.Common** is the contract package at the base of the Genocs ecosystem. It gives you public abstractions for domain modeling, CQRS, paging, notifications, startup conventions, and a small set of reusable utility types. It is designed for consumers who want to reference stable contracts from NuGet without depending on a concrete runtime, database adapter, or transport implementation.

This document is written for package consumers, including AI agents, that cannot inspect the repository source code. It focuses on what you can safely use from the public API, what this package does not do by itself, and how to combine it with other Genocs packages.

[![NuGet](https://img.shields.io/nuget/v/Genocs.Common.svg)](https://www.nuget.org/packages/Genocs.Common/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Common.svg)](https://www.nuget.org/packages/Genocs.Common/)

## Package At A Glance

| Item | Value |
|---|---|
| Package | `Genocs.Common` |
| Package type | Contract and shared-model package |
| Target frameworks | `.NET 10.0`, `.NET 9.0`, `.NET 8.0` |
| Safe assumption | Public interfaces and helper models are stable integration points |
| Unsafe assumption | Runtime dispatching, persistence, hosting, or transport behavior exists in this package |

## When To Use This Package

Use `Genocs.Common` when you need to:

- Define commands, queries, and events without choosing a dispatcher implementation yet.
- Model entities, aggregate roots, repositories, and auditing contracts in a domain layer.
- Standardize paged request and response shapes across services.
- Reference shared interfaces such as current-user access, notification sending, or background jobs.
- Share message metadata and scanning conventions through attributes and marker interfaces.

Do not use `Genocs.Common` alone when you need to:

- Execute commands, queries, or events at runtime.
- Persist entities to MongoDB, SQL Server, or another store.
- Expose HTTP endpoints or host a web application.
- Send notifications through a real transport such as SignalR or a broker.

For those concerns, combine this package with runtime packages such as `Genocs.Core`, `Genocs.Persistence.MongoDB`, `Genocs.Persistence.EFCore`, or Web API focused packages.

## Installation

```bash
dotnet add package Genocs.Common
```

## Quick Start

The quickest valid use of `Genocs.Common` is to define contracts in your application or domain layer.

```csharp
using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;
using Genocs.Common.Domain.Entities;

public sealed record CreateOrder(Guid CustomerId) : ICommand;

public sealed record GetOrder(Guid Id) : IQuery<OrderDto>;

public sealed record OrderCreated(Guid OrderId) : IEvent;

public sealed record OrderDto(Guid Id, Guid CustomerId, string Status);

public sealed class Order : IAggregateRoot<Guid>
{
    public Guid Id { get; init; }
    public List<IEvent>? DomainEvents { get; } = [];

    public bool IsTransient() => Id == Guid.Empty;
}
```

This code compiles against `Genocs.Common`, but it does not execute business workflows by itself. To run handlers, persist entities, or expose endpoints, add a runtime package that implements these contracts.

## Consumer Rules For AI Agents

If you are an AI agent using this package from NuGet and cannot inspect source code, follow these rules:

1. Treat `Genocs.Common` as a public-contract package, not a runtime framework.
2. Depend only on documented public interfaces, classes, and attributes.
3. Do not assume assembly scanning, DI registration, database access, or message transport exists unless another package is added.
4. Prefer minimal, explicit contract implementations in consumer code.
5. When behavior is ambiguous, request the names and versions of the companion Genocs packages in use.

## Public API By Job To Be Done

### Model Domain Entities And Aggregates

Use these contracts when defining your domain model:

- **`IIdentifiable<TKey>`**: Minimal identity contract.
- **`IEntity`**: Base entity contract with `IsTransient()`.
- **`IEntity<TKey>`**: Entity with typed identifier.
- **`IAggregateRoot`**: Marker interface for aggregate roots.
- **`IAggregateRoot<TKey>`**: Typed aggregate root with domain event support.
- **`IGeneratesDomainEvents`**: Exposes aggregate-generated events.
- **`ISoftDelete`**: Adds `IsDeleted` for soft deletion workflows.

Choose these auditing interfaces only when you need the corresponding metadata:

- **`IHasCreationTime`**
- **`IHasModificationTime`**
- **`IHasDeletionTime`**
- **`ICreationAudited`** and `ICreationAudited<TUser>`
- **`IModificationAudited`** and `IModificationAudited<TUser>`
- **`IDeletionAudited`** and `IDeletionAudited<TUser>`
- **`IAudited`** and `IAudited<TUser>`
- **`IFullAudited`** and `IFullAudited<TUser>`

**Guidance:**
- Use `IAggregateRoot<TKey>` for true transaction boundaries.
- Keep `DomainEvents` on aggregates, not on all entities.
- Apply full auditing only if the consumer application will actually populate the fields.

### Define Persistence Contracts


Use these contracts to define repository boundaries without committing to a database engine:

- **`IRepository<TEntity, TKey>`**: Marker repository interface.
- **`IRepositoryOfEntity<TEntity, TKey>`**: Async-first CRUD and query-oriented repository contract. All methods are asynchronous and accept a `CancellationToken`. Retrieval methods such as `GetByIdAsync` return `null` if not found, making not-found semantics explicit.
- **`IUnitOfWork`**: Commit boundary with `Task<int> Save()`.
- **`ISupportsExplicitLoading<TEntity, TPrimaryKey>`**: Explicit loading for related data.
- **`IDatabaseInitializer`**: Database startup initialization.
- **`ICustomSeeder`**: Data seeding hook.
- **`IConnectionStringValidator`**: Connection string validation. Now located in the `Genocs.Common.Persistence` namespace (moved from `Genocs.Common.Domain.ConnectionString`).
- **`IConnectionStringSecurer`**: Safe connection string masking. Now located in the `Genocs.Common.Persistence` namespace (moved from `Genocs.Common.Domain.ConnectionString`).

**Important:**
`Genocs.Common` does not implement any repository or unit-of-work behavior. It only defines the shape your infrastructure package should implement. All repository contracts are now async-only; synchronous methods have been removed for safety and modern .NET compatibility.

**Migration Guidance:**
- Update all repository implementations to remove synchronous methods and use async signatures.
- Consumers should use `await repository.GetByIdAsync(id, cancellationToken)` and handle `null` for not-found cases.

### Define Commands, Queries, And Events

Use these contracts for CQRS-oriented applications:

- **`IMessage`**: Base marker interface for messages.
- **`ICommand`**: Marker for commands.
- **`ICommandHandler<TCommand>`**: Async command handler contract.
- **`ICommandHandlerLegacy<T>`**: Legacy command handler shape.
- **`ICommandDispatcher`**: Command dispatch contract.
- **`IQuery`** and **`IQuery<TResult>`**: Query markers.
- **`IQueryHandler<TQuery, TResult>`**: Async query handler contract.
- **`IQueryDispatcher`**: Query execution contract.
- **`IEvent`**: Event marker.
- **`IEventHandler<TEvent>`**: Async event handler contract.
- **`IEventHandlerLegacy<T>`**: Legacy event handler shape.
- **`IEventDispatcher`**: Event publish contract.
- **`IDispatcher`**: Unified abstraction that composes `ICommandDispatcher`, `IQueryDispatcher`, and `IEventDispatcher`.

**Important:**
Dispatchers are interfaces only. If you have only `Genocs.Common`, you can define contracts but you cannot execute them until another package provides implementations.

### Standardize Paging And Search

Use these public types for pageable APIs and query contracts:

- **`IPagedQuery`**: Page index, page size, sorting, and order metadata.
- **`PagedQueryBase`**: Reusable base implementation for pageable request models.
- **`PagedQueryWithFilter`**: Pageable request model with a simple string filter.
- **`ISearchRequest`**: Search query contract with `SearchTerm` and `MaxItems` (`q` remains as a compatibility alias).
- **`SearchRequest`**: Basic implementation of `ISearchRequest`.
- **`PagedResultBase`**: Common response paging metadata. Throws if the requested page is out of range (negative or >= total pages).
- **`PagedResult<T>`**: Typed paged result with `Items` and helper factory methods.
- **`IPagedFilter<TResult, TQuery>`**: Filter contract that returns a `PagedResult<TResult>`.

**Guidance:**
- Use `PagedQueryBase` for request DTOs you control.
- Use `PagedResult<T>` for consistent output from read-model endpoints.
- Keep page numbering zero-based because `PagedQueryBase` and `PagedResultBase` are documented that way.

### Use Cross-Cutting Service Contracts


### Use Cross-Cutting Service Contracts

Use these interfaces when your application layer needs a stable abstraction but not a concrete implementation:

- **`ICurrentUser`**: Read authenticated user information, claims, tenant, and role membership.
- **`IDto`**: Marker interface for DTOs.
- **`IJobService`**: Enqueue, schedule, delete, and requeue background jobs. **Note:** As of March 2026, this interface no longer inherits a DI lifetime marker. Lifetime is now an infrastructure concern.
- **`INotificationSender`**: Send notifications to all users, groups, or selected users. **Note:** As of March 2026, this interface no longer inherits a DI lifetime marker. Lifetime is now an infrastructure concern.
- **`ISerializerService`**: Serialize and deserialize values. **Note:** As of March 2026, this interface no longer inherits a DI lifetime marker. Lifetime is now an infrastructure concern.

**Important:**
These are not built-in services. The package assumes your host application or another Genocs package will provide implementations. Service lifetimes should be assigned at registration time in your DI container, not by interface inheritance.

### Use Notification Models

These types standardize notification payloads:

- **`INotificationMessage`**: Base notification contract.
- **`BasicNotification`**: Simple message with severity label.
- **`BasicNotification.LabelType`**: `Information`, `Success`, `Warning`, `Error`.
- **`JobNotification`**: Notification with `Message`, `JobId`, and `Progress`.
- **`StatsChangedNotification`**: Marker notification type.
- **`NotificationConstants`**: Shared notification constants such as `NotificationFromServer`.

### Use Conventions, Startup Hooks, And Metadata


Use these types to support convention-based composition:

- **`ISingletonDependency`**: Marker for singleton registration conventions (canonical, in `Genocs.Common.Dependency`).
- **`ITransientDependency`**: Marker for transient registration conventions (canonical, in `Genocs.Common.Dependency`).
- **`IScopedDependency`**: Marker for scoped registration (canonical, in `Genocs.Common.Dependency`).
- **`IInitializer`**: Boot-time initialization contract.
- **`IStartupInitializer`**: Aggregates multiple initializers.
- **`MessageAttribute`**: Message metadata for exchange, topic, queue, queue type, error queue, and subscription ID.
- **`DecoratorAttribute`**: Decorator marker for scanners or registration rules.
- **`HiddenAttribute`**: Property-level hide marker.
- **`PublicContractAttribute`**: Marks a class as a public contract.

**Obsolete:**
- `IScopedService` and `ITransientService` (in `Genocs.Common.Interfaces`) are now obsolete and inherit from the canonical markers. Use only the canonical marker interfaces for new code.

**Important:**
The attributes do not perform behavior on their own. They become meaningful only when another package or your own code reads them.

### Use Shared Utility Types

-These types are small but useful for consumers:

- **`AppOptions`**: Shared options model for the `app` configuration section. Properties are immutable after binding (init-only).
- **`IServiceId`** and **`ServiceId`**: Per-instance GUID-based service identity. Now located in the `Genocs.Common.Services` namespace (moved from `Genocs.Common.Builders`).
- **`ITypeList`** and **`TypeList`**: Type collections constrained to a base type.
- **`Extensions`**: Reflection-based helpers that create default instances and populate default property values.

## Configuration

`AppOptions` maps the `app` section.

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

| Setting | Type | Purpose |
|---|---|---|
| `enabled` | `bool` | Enables use of the `app` section by consuming packages. |
| `name` | `string` | Human-readable application name. |
| `service` | `string` | Stable service identifier. |
| `instance` | `string` | Running instance identifier. |
| `version` | `string` | Application version shown in diagnostics. |
| `displayBanner` | `bool` | Allows banner display when supported by the runtime package. |
| `displayVersion` | `bool` | Allows startup version display when supported by the runtime package. |

## What This Package Does Not Provide

To avoid incorrect assumptions, do not infer these capabilities from `Genocs.Common` alone:

- No command, query, or event dispatcher implementation.
- No repository implementation.
- No built-in database provider integration.
- No automatic DI registration behavior by itself.
- No HTTP hosting or endpoint mapping.
- No notification transport.
- No job scheduler implementation.

## Composition Guide

Pair `Genocs.Common` with other packages based on your goal:

| Goal | Add |
|---|---|
| Execute CQRS handlers in-process | `Genocs.Core` |
| Implement repositories on MongoDB | `Genocs.Persistence.MongoDB` |
| Implement repositories on EF Core | `Genocs.Persistence.EFCore` |
| Expose CQRS through HTTP endpoints | `Genocs.WebApi` and `Genocs.WebApi.CQRS` |
| Add messaging infrastructure | A Genocs messaging package that matches your broker |

## Common Recipes

### Recipe 1: Define A Command Contract

```csharp
using Genocs.Common.CQRS.Commands;

public sealed record CreateCustomer(string Email, string Name) : ICommand;
```

Use this when you want to model a write operation without choosing the runtime dispatcher yet.

### Recipe 2: Define A Query Contract With A Typed Result

```csharp
using Genocs.Common.CQRS.Queries;

public sealed record GetCustomer(Guid Id) : IQuery<CustomerDto>;

public sealed record CustomerDto(Guid Id, string Email, string Name);
```

Use this when you want a query contract that remains independent from controllers, endpoints, or transport details.

### Recipe 3: Create A Pageable Query Model

```csharp
using Genocs.Common.CQRS.Queries;

public sealed class BrowseCustomers : PagedQueryBase, IQuery<PagedResult<CustomerDto>>
{
    public string? EmailDomain { get; init; }
}
```

Use this when multiple endpoints should share a common paging model.

### Recipe 4: Return A Paged Result

```csharp
using Genocs.Common.CQRS.Queries;

var result = PagedResult<CustomerDto>.Create(
    items,
    currentPage: 0,
    resultsPerPage: 20,
    totalPages: 4,
    totalResults: 72);
```

Use this when you need a consistent response contract for list or search operations.

### Recipe 5: Model An Aggregate Root

```csharp
using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;

public sealed class Customer : IAggregateRoot<Guid>
{
    public Guid Id { get; init; }
    public List<IEvent>? DomainEvents { get; } = [];

    public bool IsTransient() => Id == Guid.Empty;
}
```

Use this when your domain model needs aggregate-level event collection but not a framework base class.

## Failure Modes And Safe Responses

### Contracts Compile But Nothing Executes

Cause:
Only `Genocs.Common` is installed, so there are no dispatcher or handler implementations.

Safe response:
Add a runtime package such as `Genocs.Core` or a custom implementation that registers `ICommandDispatcher`, `IQueryDispatcher`, and `IEventDispatcher`.

### Repository Interface Exists But No Data Is Persisted

Cause:
`IRepositoryOfEntity<TEntity, TKey>` is a contract only.

Safe response:
Add a persistence package and implement the repository interfaces for your storage engine.

### Notification Contracts Exist But No Client Receives Messages

Cause:
`INotificationSender` is only an abstraction.

Safe response:
Register a concrete sender implementation and the required transport infrastructure.

### Auditing Properties Stay Empty

Cause:
Auditing interfaces declare fields but do not populate them.

Safe response:
Populate these values in your application, persistence, or pipeline layer.

## Best Practices

- Keep Genocs.Common in the domain or application contract layer, not in infrastructure-only code.
- Implement only the interfaces you need. Do not add auditing or soft-delete contracts by default.
- Prefer `IQuery<TResult>` plus `PagedResult<T>` for read models instead of ad hoc paging shapes.
- Use marker interfaces and attributes only if another package or your own conventions read them.
- Keep public contracts stable once multiple services or agents depend on them.

## Related Libraries

- **Genocs.Core**: Runtime implementations for many Genocs.Common contracts.
- **Genocs.WebApi**: HTTP and endpoint integration for Genocs-based services.
- **Genocs.WebApi.CQRS**: CQRS integration for Web API endpoints.
- **Genocs.Persistence.MongoDB**: MongoDB-backed persistence implementations.
- **Genocs.Persistence.EFCore**: EF Core-backed persistence implementations.

## Support And Documentation

- **Documentation**: [https://learn.fiscanner.net/](https://learn.fiscanner.net/)
- **Source Code**: [https://github.com/Genocs/genocs-library](https://github.com/Genocs/genocs-library)
- **Issues**: [https://github.com/Genocs/genocs-library/issues](https://github.com/Genocs/genocs-library/issues)
- **Changelog**: [https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md](https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md)

## License

This library is released under the MIT License. See [LICENSE](https://github.com/Genocs/genocs-library/blob/main/LICENSE) file for details.

## Contributing

Contributions are welcome. Read the [Code of Conduct](https://github.com/Genocs/genocs-library/blob/main/CODE_OF_CONDUCT.md) before submitting pull requests.
