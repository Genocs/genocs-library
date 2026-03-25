# Genocs.Common Agent Reference

## Agent Operating Mode

- Assume `Genocs.Common` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented public interfaces, classes, and attributes as the only safe API surface.
- If runtime behavior is required, identify the companion Genocs packages before making changes.

## Purpose

`Genocs.Common` is a shared contract package. It defines public abstractions for CQRS, domain entities, repositories, paging, notifications, startup conventions, and utility models. It does not provide runtime implementations for those abstractions.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Common` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Contract and shared-model package |
| Safe use | Define public contracts and common DTO-like models |
| Unsafe assumption | Handler execution, repository behavior, or transport integration exists in this package |

## Install

```bash
dotnet add package Genocs.Common
```

## Use This Package To

- Define `ICommand`, `IQuery<TResult>`, and `IEvent` contracts.
- Define `IEntity<TKey>` and `IAggregateRoot<TKey>` domain contracts.
- Standardize paging with `IPagedQuery`, `PagedQueryBase`, and `PagedResult<T>`.
- Reference stable abstractions such as `ICurrentUser`, `INotificationSender`, and `IJobService`.
- Apply metadata attributes such as `MessageAttribute` and `PublicContractAttribute`.

## Do Not Assume This Package Can

- Execute command, query, or event handlers.
- Persist entities.
- Register itself automatically into DI.
- Send notifications over a transport.
- Schedule jobs.
- Host HTTP endpoints.

## Minimal Valid Usage

```csharp
using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;

public sealed record CreateBook(string Title) : ICommand;

public sealed record GetBook(Guid Id) : IQuery<BookDto>;

public sealed record BookDto(Guid Id, string Title);
```

This is valid package usage. It defines contracts only. It does not provide runtime execution.

## Decision Matrix For Agents

| If you need to... | Use |
|---|---|
| Model a command contract | `ICommand` |
| Model a query contract | `IQuery<TResult>` |
| Model an event contract | `IEvent` |
| Model a unified dispatcher dependency | `IDispatcher` |
| Standardize paged request input | `IPagedQuery` or `PagedQueryBase` |
| Standardize paged response output | `PagedResult<T>` |
| Model an aggregate root | `IAggregateRoot<TKey>` |
| Model notification payloads | `INotificationMessage`, `BasicNotification`, `JobNotification` |

## Public Capability Map

- CQRS contracts: `ICommand`, `IQuery<TResult>`, `IEvent`, `IDispatcher`
- Handler contracts: `ICommandHandler<TCommand>`, `IQueryHandler<TQuery, TResult>`, `IEventHandler<TEvent>`
- Domain contracts: `IEntity<TKey>`, `IAggregateRoot<TKey>`, auditing interfaces, `ISoftDelete`
- Persistence contracts: `IRepositoryOfEntity<TEntity, TKey>`, `IUnitOfWork`, `ISupportsExplicitLoading<TEntity, TPrimaryKey>`
- Paging models: `IPagedQuery`, `PagedQueryBase`, `PagedQueryWithFilter`, `PagedResult<T>`
- Service abstractions: `ICurrentUser`, `INotificationSender`, `ISerializerService`, `IJobService`
- Metadata and conventions: `MessageAttribute`, `DecoratorAttribute`, `HiddenAttribute`, `PublicContractAttribute`

## Configuration

`AppOptions` maps the shared `app` section.

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

This package does not enforce or read this configuration by itself. Companion packages may do so.

## Safe Planning Rules

1. If only `Genocs.Common` is referenced, generate contracts, not runtime wiring.
2. If a user asks for handler execution, ask which runtime package implements the dispatchers.
3. If a user asks for persistence, ask which storage adapter package is installed.
4. If a user asks for notifications or jobs, ask for the concrete implementation package or service.
5. Prefer explicit contracts over inferred conventions when package composition is unclear.

## Troubleshooting

1. Contracts compile but no business logic runs.
Fix: Add a runtime package that implements and registers dispatchers and handlers.

2. Repository interfaces exist but nothing persists.
Fix: Add a persistence package such as `Genocs.Persistence.MongoDB` or `Genocs.Persistence.EFCore`.

3. Notifications are modeled but no client receives them.
Fix: Register a concrete `INotificationSender` implementation and its transport infrastructure.

4. Auditing contracts are implemented but fields remain empty.
Fix: Populate those values in application logic, middleware, or persistence infrastructure.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.WebApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Persistence.MongoDB`
- `Genocs.Persistence.EFCore`


