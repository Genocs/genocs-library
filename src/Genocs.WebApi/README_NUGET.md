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

## Main Entry Points

- `AddWebApi`
- `UseEndpoints`
- `UseErrorHandler`
- `UseAllForwardedHeaders`

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
