# Genocs.WebApi.CQRS

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

CQRS extensions for Genocs ASP.NET Core Web APIs. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.WebApi.CQRS
```

## Getting Started

Use this package to expose CQRS dispatcher endpoints and optional public contracts endpoints in Web API hosts.

## Main Entry Points

- `AddInMemoryDispatcher`
- `MapDispatcherEndpoints`
- `UsePublicContracts`

## Migration: UseDispatcherEndpoints -> MapDispatcherEndpoints

`UseDispatcherEndpoints(...)` is a legacy API and is now obsolete.

Use route-builder mapping so host middleware ordering stays owned by the application:

Before:

```csharp
app.UseDispatcherEndpoints(endpoints => endpoints
	.Post<CreateOrder>("/orders")
	.Get<GetOrder, OrderDto>("/orders/{id}"));
```

After:

```csharp
app.MapDispatcherEndpoints(endpoints => endpoints
	.Post<CreateOrder>("/orders")
	.Get<GetOrder, OrderDto>("/orders/{id}"));
```

Host pipeline concerns such as `UseRouting()`, `UseAuthorization()`, and other middleware should now be configured explicitly in host startup.

Callback signatures are now non-null and cancellation-aware:

- command hooks: `(cmd, httpContext, cancellationToken)`
- query hooks: `(query, httpContext, cancellationToken)` and `(query, result, httpContext, cancellationToken)`

Dispatch and helper extensions propagate `HttpContext.RequestAborted` to command/query dispatchers.

## Validation

Run package-level validation from repository root:

```bash
make validate-webapi-cqrs
```

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
