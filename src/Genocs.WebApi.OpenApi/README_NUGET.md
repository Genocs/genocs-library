# Genocs OpenApi

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Swagger/OpenAPI integration for documenting Genocs Web APIs. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.WebApi.OpenApi
```

## Getting Started

Use this package to register OpenAPI generation and expose Swagger UI/document endpoints in Web API hosts.

## Main Entry Points

- `AddOpenApiDocs`
- `UseOpenApiDocs`

## Fluent Builder Coverage

`IOpenApiOptionsBuilder` supports the complete `OpenApiOptions` surface, including:

- API metadata: name, title, version, description, route prefix
- contact metadata: name, email, and URL
- license metadata: name and URL
- terms of service URL/text
- server list via `WithServers(...)` or `AddServer(...)`
- security inclusion via `IncludeSecurity(...)`

## Maintainer Validation

Use these commands to validate package-local quality gates:

```bash
dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -f net10.0 -c Debug --nologo -warnaserror -p:BuildProjectReferences=false
dotnet test src/tests/Genocs.WebApi.OpenApi.UnitTests/Genocs.WebApi.OpenApi.UnitTests.csproj -c Debug --nologo
make validate-webapi-openapi
```

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases