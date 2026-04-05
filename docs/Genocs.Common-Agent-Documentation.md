# Genocs.Common Agent Reference

## Agent Operating Mode

- Assume `Genocs.Common` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat the package as a contracts-only dependency unless the user explicitly confirms companion runtime packages.
- Generate abstractions, contracts, DTO-like models, and metadata usage safely.
- Do not generate runtime wiring, transport code, or persistence behavior unless another package is confirmed.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Common` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Shared contracts, primitives, and cross-package models |
| Main value | Stable CQRS, DDD, repository, paging, notification, startup, and metadata abstractions |
| Runtime implementations included | None |

## What This Package Is For

Use `Genocs.Common` when you need to:

- define commands, queries, events, and handler contracts
- define entities, aggregate roots, soft-delete, and auditing contracts
- standardize repository and unit-of-work abstractions
- standardize paging and search request and response models
- reference common service abstractions such as current-user, serializer, jobs, and notifications
- attach metadata to contracts through attributes such as `MessageAttribute` and `PublicContractAttribute`

## What This Package Does Not Do By Itself

Do not assume `Genocs.Common` can:

- execute commands, queries, or events
- persist entities or connect to a database
- register services into dependency injection automatically
- host HTTP endpoints or middleware
- send notifications over SignalR, email, SMS, or a broker
- schedule background jobs
- populate audit values automatically
- provide a concrete current-user implementation

## Safe Default Mental Model

Treat `Genocs.Common` as four things:

1. A contract boundary between domain or application code and infrastructure
2. A shared language for CQRS and DDD modeling
3. A place to define reusable request, response, and notification models
4. A convention layer that companion packages may interpret at runtime

If a user asks for behavior rather than contracts, identify the missing runtime package before generating code.

## Fast Start Recipes

### Recipe 1: Define A Command And Query Contract

Use this when a project needs application-layer contracts only.

```csharp
using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;

public sealed record CreateBook(string Title) : ICommand;

public sealed record GetBook(Guid Id) : IQuery<BookDto>;

public sealed record BookDto(Guid Id, string Title);
```

Effect:
- defines compile-time contracts only
- does not provide execution behavior
- stays safe for shared domain or application packages

### Recipe 2: Model An Aggregate Root Contract

Use this when the domain layer needs identities and domain events without runtime coupling.

```csharp
using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;

public sealed class Order : IAggregateRoot<Guid>
{
    public Guid Id { get; init; }
    public List<IEvent>? DomainEvents { get; } = [];

    public bool IsTransient() => Id == Guid.Empty;
}
```

Use this when the type must stay infrastructure-agnostic.

### Recipe 3: Standardize Paging Contracts

Use this when multiple services or endpoints should share the same paging model.

```csharp
using Genocs.Common.CQRS.Queries;

public sealed record GetOrders : PagedQueryBase, IQuery<PagedResult<OrderDto>>;

public sealed record OrderDto(Guid Id, decimal Total);
```

Use this when consistency matters more than runtime behavior.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `ICommand` | Model a write request | Marker interface only | Assuming it carries execution logic |
| `IQuery<TResult>` | Model a typed read request | Declares expected result type | Assuming a dispatcher exists automatically |
| `IEvent` | Model a domain or integration event | Marker interface only | Treating it as brokered messaging by itself |
| `ITransactionalEvent` | Mark integration events for durable outbox intent | Contract-only marker | Assuming it provides storage or dispatch behavior |
| `IOutboxMessage<TIntegrationEvent>` | Model durable publication intent envelope | Includes message id, correlation id, timestamp, and integration payload contract | Binding it to a concrete broker or serializer in shared contracts |
| `IOutboxDispatcher` | Depend on durable outbox enqueue abstraction | Requires runtime implementation package | Assuming enqueue behavior exists with `Genocs.Common` alone |
| `ICommandHandler<TCommand>` | Define command handling contract | No implementation included | Expecting DI registration from this package |
| `IQueryHandler<TQuery, TResult>` | Define query handling contract | Async contract only | Assuming query dispatch is available |
| `IEventHandler<TEvent>` | Define event handling contract | Async contract only | Assuming publish-subscribe infrastructure exists |
| `IRejectedEvent` and `RejectedEvent` | Represent integration-style failures | Prefer structured codes (`category.subject.reason`) and shared `Error` alignment | Using unstructured ad hoc codes that are hard to classify |
| `IDispatcher` and dispatcher interfaces | Depend on an abstraction | Requires a runtime package to work | Attempting to resolve one without a companion package |
| `IAggregateRoot<TKey>` | Model a consistency boundary | Includes domain event support | Using it for every entity |
| `IEntity` | Check persistence lifecycle state | Prefer `IsNew()` for new code; `IsTransient()` remains for compatibility | Confusing entity lifecycle checks with DI service lifetimes |
| `IVersioned` | Expose optimistic concurrency token | Minimal `long Version` contract for compare-and-swap persistence flows | Encoding persistence-provider concurrency details into shared contracts |
| `IRepositoryOfEntity<TEntity, TKey>` | Define the default persistence boundary | Provider-agnostic contract; no `IQueryable` exposure | Depending on it for provider-backed query composition |
| `IQueryableRepository<TEntity, TKey>` | Opt into provider-backed querying | Use only in infrastructure/composition layers that intentionally expose `IQueryable<TEntity>` | Pulling it into general domain or application contracts |
| `ISpecification<TEntity>` | Model provider-neutral query intent | Encodes filter/include/order/paging intent without provider APIs | Treating it as an executable query engine |
| `ISpecificationRepository<TEntity, TKey>` | Query via specifications | Provider-agnostic composition surface for advanced queries | Replacing all simple CRUD dependencies with specification-only flows |
| `PagedQueryBase` | Reuse paging request shape | Zero-based page numbering with defaults (`Page=0`, `Results=10`); recommended `Results` max is 100 | Assuming page 1 is the first page or skipping downstream bounds validation |
| `CursorQueryBase` | Reuse cursor request shape | Opaque cursor token + limit defaults (`Limit=10`) | Parsing cursor tokens in shared contract code |
| `CursorPagedResult<T>` | Standardize cursor-window responses | Carries opaque next/previous tokens and window count | Treating cursor tokens as provider-neutral structured objects |
| `ISoftDeleteFilter` and `SoftDeleteFilterBase` | Standardize soft-delete visibility intent | `IncludeDeleted` defaults to hidden-deleted behavior | Ignoring request-level deleted visibility intent in query handlers |
| `PagedResult<T>` | Standardize paged results | Factory helpers available | Treating it as a database paging engine |
| `ValidationResult` and `ValidationError` | Standardize validation failures | Library-neutral failure shape for pipelines and APIs | Reusing framework-specific validation objects in domain contracts |
| `IValidator<T>` | Define async validation contract | Contract-only abstraction (`ValidateAsync`) | Expecting built-in FluentValidation behavior from `Genocs.Common` |
| `MessageAttribute` | Annotate message contracts with transport metadata | Pure metadata | Expecting transport behavior from the attribute |
| `IInitializer` | Define startup work contract | No executor included here | Assuming startup will run automatically |

## Choosing The Right Surface

| Situation | Prefer |
|---|---|
| You only need compile-time CQRS contracts | `ICommand`, `IQuery<TResult>`, `IEvent` |
| You need a shared paged query shape | `PagedQueryBase` or `PagedQueryWithFilter` |
| You need cursor-based request/response contracts | `ICursorQuery`, `CursorQueryBase`, and `CursorPagedResult<T>` |
| You need a standard soft-delete visibility switch | `ISoftDeleteFilter` or `SoftDeleteFilterBase` |
| You need a common paged response | `PagedResult<T>` |
| You need standardized validation failures in pipelines | `ValidationResult`, `ValidationError`, and optionally `IValidator<T>` |
| You need optimistic-concurrency token contracts | `IVersioned` |
| You need a persistence abstraction | `IRepositoryOfEntity<TEntity, TKey>` and `IUnitOfWork` |
| You need provider-backed query composition | `IQueryableRepository<TEntity, TKey>` |
| You need provider-neutral advanced query composition | `ISpecificationRepository<TEntity, TKey>` with `ISpecification<TEntity>` |
| You need a shared authenticated-user abstraction | `ICurrentUser` |
| You need metadata for messages | `MessageAttribute` |
| You need runtime behavior | Ask for `Genocs.Core` or another concrete companion package |

## Public Capability Map

### CQRS Contracts

- `IMessage`
- `ICommand`
- `ICommandHandler<TCommand>`
- `ICommandDispatcher`
- `IQuery<TResult>`
- `IQueryHandler<TQuery, TResult>`
- `IQueryDispatcher`
- `IEvent`
- `IIntegrationEvent`
- `ITransactionalEvent`
- `IOutboxMessage<TIntegrationEvent>`
- `IOutboxMessage`
- `IOutboxDispatcher`
- `IEventHandler<TEvent>`
- `IEventDispatcher`
- `IDispatcher`
- `IRejectedEvent` and `RejectedEvent`

### Domain And Auditing Contracts

- `IIdentifiable<TKey>`
- `IEntity<TKey>`
- `IAggregateRoot<TKey>`
- `IGeneratesDomainEvents`
- `IVersioned`
- `ISoftDelete`
- `IHasCreationTime`, `IHasModificationTime`, `IHasDeletionTime`
- `ICreationAudited`, `IModificationAudited`, `IDeletionAudited`
- `IAudited`, `IFullAudited`

### Persistence Contracts

- `IRepository<TEntity, TKey>`
- `IRepositoryOfEntity<TEntity, TKey>`
- `IQueryableRepository<TEntity, TKey>`
- `ISpecification<TEntity>`
- `IProjectionSpecification<TEntity, TResult>`
- `ISpecificationRepository<TEntity, TKey>`
- `IUnitOfWork`
- `ISupportsExplicitLoading<TEntity, TPrimaryKey>`
- `IDatabaseInitializer`
- `ICustomSeeder`
- `IConnectionStringValidator`
- `IConnectionStringSecurer`

### Paging And Search

- `IPagedQuery`
- `PagedQueryBase`
- `PagedQueryWithFilter`
- `ICursorQuery`
- `CursorQueryBase`
- `ISoftDeleteFilter`
- `SoftDeleteFilterBase`
- `ISearchRequest`
- `SearchRequest`
- `PagedResultBase`
- `PagedResult<T>`
- `CursorPagedResult<T>`
- `IPagedFilter<TResult, TQuery>`

### Validation Contracts

- `ValidationError`
- `ValidationResult`
- `IValidator<T>`

### Service Abstractions

- `ICurrentUser`
- `ISerializerService`
- `IJobService`
- `INotificationSender`
- `INotificationMessage`
- `IDto`

### Conventions And Metadata

- `IInitializer`
- `IStartupInitializer`
- `ISingletonDependency`
- `ITransientDependency`
- `IScopedService`
- `ITransientService`
- `MessageAttribute`
- `DecoratorAttribute`
- `HiddenAttribute`
- `PublicContractAttribute`
- `AppOptions`
- `IServiceId` and `ServiceId`
- `ITypeList<T>` and `TypeList<T>`

## Configuration Ownership

`Genocs.Common` defines the shared `app` configuration model through `AppOptions`.

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

What this package provides:

- the `AppOptions` type
- a stable shape for shared application metadata

What this package does not provide:

- automatic binding
- automatic validation
- startup-time consumption of the section

Those behaviors come from companion packages such as `Genocs.Core`.

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. If only `Genocs.Common` is confirmed, generate contracts only.
2. Do not resolve dispatcher interfaces unless a runtime implementation package is confirmed.
3. Do not generate repository implementations unless a persistence package is confirmed.
4. Do not treat marker interfaces as active DI behavior unless a scanner is confirmed.
5. Do not assume auditing contracts populate values automatically.
6. Do not assume `MessageAttribute` creates broker subscriptions or queue bindings.
7. Do not assume notification or job abstractions have a transport or scheduler behind them.
8. Do not assume `AppOptions` is automatically bound into configuration.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Common`, answer these questions:

1. Is the user asking for contracts only, or for runnable behavior?
2. Is `Genocs.Core` or another runtime Genocs package also installed?
3. Is a persistence package such as `Genocs.Persistence.MongoDB` or `Genocs.Persistence.EFCore` installed?
4. Is a transport implementation available for notifications or events?
5. Does the host use scanning conventions for marker interfaces, or explicit DI registration?
6. Does the consumer expect zero-based or one-based paging input?

If any of these answers are unknown, prefer abstractions and ask for the missing package detail.

## Common Tasks And Safe Responses

### Task: "Set up CQRS"

Safe response:
- define commands, queries, events, and handler interfaces
- mention that execution requires `Genocs.Core` or another runtime implementation

### Task: "Add repository support"

Safe response:
- define repository interfaces in the domain or application layer
- ask which persistence package should implement them

### Task: "Add auditing"

Safe response:
- apply only the auditing interfaces the system actually intends to populate
- avoid promising automatic audit behavior

### Task: "Add notifications"

Safe response:
- define notification contracts and payloads
- ask which transport or concrete `INotificationSender` implementation is available

### Task: "Register services with marker interfaces"

Safe response:
- mention that marker interfaces are conventions only
- ask whether a scanner or registration package is already in place

## Failure Modes And Troubleshooting

1. Contracts compile but nothing runs.
Fix: Add a runtime package such as `Genocs.Core` that implements and registers dispatchers and startup behavior.

2. Repository interfaces exist but no data is persisted.
Fix: Add a persistence package such as `Genocs.Persistence.MongoDB` or `Genocs.Persistence.EFCore`.

3. Notifications are modeled but no consumer receives them.
Fix: Register a concrete `INotificationSender` implementation and its underlying transport.

4. Job abstractions exist but no background work is scheduled.
Fix: Integrate a concrete scheduler or job-service implementation.

5. Auditing properties remain empty.
Fix: Populate those values in handlers, middleware, interceptors, or persistence infrastructure.

6. Page numbering looks off by one.
Fix: Re-check the client contract; `PagedQueryBase` is zero-based.

7. Marker interfaces are present but DI resolution still fails.
Fix: Confirm that the host uses a scanner or register the services explicitly.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.WebApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Persistence.MongoDB`
- `Genocs.Persistence.EFCore`
- `Genocs.Persistence.Redis`
- `Genocs.Messaging`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Auth`
- `Genocs.Logging`
- `Genocs.Telemetry`

## One-Line Recommendation For Agents

If you only know that `Genocs.Common` is installed, generate stable contracts and shared models only. Ask before generating runtime execution, persistence, transport, or host integration code.