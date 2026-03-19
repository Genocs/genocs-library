# Genocs.Auth

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Authentication and authorization helpers for ASP.NET Core applications. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Auth
```

## Getting Started

Use this package to configure JWT/OpenID authentication flows and access-token validation in ASP.NET Core services.

Service registration:

```csharp
using Genocs.Auth;

genocs.AddJwt();
```

Pipeline setup:

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseAccessTokenValidator();
```

## Main Entry Points

- `AddJwt`
- `AddOpenIdJwt`
- `AddPrivateKeyJwt`
- `UseAccessTokenValidator`

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases