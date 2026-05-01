# Genocs.WebApi

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

ASP.NET Core Web API extensions and conventions for Genocs applications. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.WebApi
```

## Getting Started

Use this package to register Web API conventions, endpoint mapping, forwarded headers handling, and unified error handling.

Service registration:

```csharp
using Genocs.WebApi;

genocs.AddWebApi();
```

Pipeline setup:

```csharp
app.UseErrorHandler();
app.UseAllForwardedHeaders();
app.UseEndpoints(endpoints =>
{
    // Register endpoints here
});
```

Forwarded headers safety:

- `UseAllForwardedHeaders()` is strict by default and keeps ASP.NET Core trusted network/proxy checks in place.
- Use `UseAllForwardedHeaders(resetKnownNetworksAndProxies: true)` only when your deployment edge is explicitly trusted and controlled.

Endpoint authorization metadata:

- Endpoints mapped with `IEndpointsBuilder` do not receive implicit anonymous metadata by default.
- Use `auth`, `roles`, or `policies` parameters for authorization requirements.
- If explicit anonymous metadata is required, add it through the endpoint convention callback.

## Main Entry Points

- `AddWebApi`
- `UseEndpoints`
- `UseErrorHandler`
- `UseAllForwardedHeaders`

## Validation

Use the package quality gate to validate warning baseline and regression tests:

```bash
make validate-webapi
```

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
