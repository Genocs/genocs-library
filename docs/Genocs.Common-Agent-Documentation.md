# Genocs.Common Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet
- Do not rely on repository source code access
- Prefer stable public APIs and extension methods documented here
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

`Genocs.Common` provides reusable contracts and primitives for CQRS, domain modeling, paging, and cross-cutting concerns.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Common` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Shared contracts and foundational types |
| Core entry points | `ICommand`, `IQuery<TResult>`, `IEvent`, `IPagedQuery`, `PagedResult<T>` |

## Install

```bash
dotnet add package Genocs.Common
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;

var builder = WebApplication.CreateBuilder(args);

public sealed record CreateBook(string Title) : ICommand;
public sealed record GetBook(Guid Id) : IQuery<BookDto>;
public sealed record BookDto(Guid Id, string Title);

var app = builder.Build();
app.Run();
```

## Configuration

`Genocs.Common` exposes the shared `app` section through `AppOptions`.

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

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Enables use of the section by modules that consume `AppOptions`. |
| `name` | `string` | Human-readable application name. |
| `service` | `string` | Service identifier used by infrastructure and diagnostics integrations. |
| `instance` | `string` | Instance identifier for the running process or container. |
| `version` | `string` | Application version exposed to logs, banners, and diagnostics. |
| `displayBanner` | `bool` | Shows the startup banner when supported by the host. |
| `displayVersion` | `bool` | Shows the configured version at startup. |

This package does not enforce the section itself, but downstream packages such as logging, telemetry, and hosting conventions commonly read `app` metadata.

## Decision Matrix For Agents

| If you need to... | Use |
|---|---|
| Model a command contract | `ICommand` or `ICommand<TResult>` |
| Model a query contract | `IQuery<TResult>` |
| Model an integration or domain event contract | `IEvent` |
| Standardize paged request input | `IPagedQuery` or `PagedQueryBase` |
| Standardize paged response output | `PagedResult<T>` |

## Behavior Notes / Constraints

- This package exposes contracts and shared models; it does not register runtime dispatch services by itself.
- Command/query/event execution requires a runtime package that provides handlers and dispatchers.
- Consistent identifier conventions across contracts reduce mapping and persistence issues.

## Public Capability Map

- CQRS contracts: `ICommand`, `ICommand<TResult>`, `IQuery<TResult>`, `IEvent`
- Handler contracts: command/query/event handler interfaces used by runtime modules
- Paging models: `IPagedQuery`, `PagedQueryBase`, `PagedResult<T>`
- Shared abstractions: domain and user-context interfaces for cross-module consistency

## Dependencies

- `MediatR.Contracts`
- `Humanizer.Core`
- `NetArchTest.Rules`

## Troubleshooting

1. Contracts compile but no business logic runs.
Fix: Add a runtime package that registers handlers and dispatchers for your command/query/event contracts.
2. Pagination shapes differ across endpoints.
Fix: Normalize request and response contracts on `IPagedQuery` and `PagedResult<T>`.
3. Domain objects use mixed identifier patterns.
Fix: Standardize a single ID strategy and apply it consistently across contracts and entities.


