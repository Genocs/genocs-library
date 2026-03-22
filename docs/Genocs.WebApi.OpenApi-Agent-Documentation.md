# Genocs.WebApi.OpenApi — Agent Reference Documentation

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request package version and configuration details.

## Purpose

`Genocs.WebApi.OpenApi` adds OpenAPI document generation and UI hosting (Swagger UI or ReDoc) for Genocs-based Web API services.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.WebApi.OpenApi` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | OpenAPI/Swagger integration for Genocs hosts |
| Main APIs | `AddOpenApiDocs` overloads and `UseOpenApiDocs` |

## Install

```bash
dotnet add package Genocs.WebApi.OpenApi
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.WebApi.OpenApi;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder
        .AddGenocs()
        .AddWebApi()
        .AddOpenApiDocs();

gnxBuilder.Build();

var app = builder.Build();
app.UseOpenApiDocs();
app.Run();
```

## Configuration

Use the `openapi` section.

```json
{
    "openapi": {
        "enabled": true,
        "reDocEnabled": false,
        "name": "v1",
        "title": "Orders API",
        "version": "1.0.0",
        "description": "OpenAPI description for the Orders service.",
        "routePrefix": "docs",
        "contactName": "Platform Team",
        "contactEmail": "platform@example.com",
        "contactUrl": "https://example.com/platform",
        "licenseName": "MIT",
        "licenseUrl": "https://opensource.org/licenses/MIT",
        "termsOfService": "https://example.com/terms",
        "includeSecurity": true,
        "servers": [
            {
                "url": "https://api.example.com/orders",
                "description": "Production"
            }
        ]
    }
}
```

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Enables OpenAPI service registration. |
| `reDocEnabled` | `bool` | Switches UI rendering from Swagger UI to ReDoc. |
| `name` | `string` | Document name used by Swagger generation, commonly `v1`. |
| `title` | `string` | API title shown in generated documentation. |
| `version` | `string` | API version string embedded in the document metadata. |
| `description` | `string` | API description shown in the docs UI. |
| `routePrefix` | `string` | Base route for the JSON document and UI assets. |
| `contactName` | `string` | Contact person or team name. |
| `contactEmail` | `string` | Contact email. |
| `contactUrl` | `string` | Contact URL. |
| `licenseName` | `string` | API license name. |
| `licenseUrl` | `string` | API license URL. |
| `termsOfService` | `string` | Terms of service URL or text reference. |
| `includeSecurity` | `bool` | Adds Bearer/JWT security metadata to the generated document. |
| `servers[].url` | `string` | Server URL advertised by the generated OpenAPI document. |
| `servers[].description` | `string` | Human-readable label for each server. |

If XML comments are enabled for the host assembly, they are included in the generated document. Keep the XML documentation file available in the application output when `openapi.enabled` is turned on.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Register OpenAPI services from configuration | `AddOpenApiDocs()` |
| Register OpenAPI services from fluent builder | `AddOpenApiDocs(builder => ...)` |
| Activate docs endpoints and UI | `UseOpenApiDocs()` |
| Add Bearer security definition in docs | Set `openapi.includeSecurity = true` |
| Serve ReDoc instead of Swagger UI | Set `openapi.reDocEnabled = true` |

## Behavior Notes / Constraints

- Service registration and middleware activation are separate steps.
- If docs are disabled in settings, middleware activation returns without serving docs.
- XML comment integration depends on host XML documentation file availability.

## Public Capability Map

- OpenAPI service registration via `AddOpenApiDocs` overloads.
- Runtime docs middleware via `UseOpenApiDocs`.
- Configuration model for document metadata, route prefix, and security options.
- Endpoint metadata enrichment through package document filtering support.

## Dependencies

- `Genocs.WebApi`
- `Swashbuckle.AspNetCore.Annotations`
- `Swashbuckle.AspNetCore.Swagger`
- `Swashbuckle.AspNetCore.SwaggerGen`
- `Swashbuckle.AspNetCore.SwaggerUI`
- `Swashbuckle.AspNetCore.ReDoc`

## Troubleshooting

1. Swagger or ReDoc endpoints are missing.
Fix: Enable `openapi.enabled` and ensure `UseOpenApiDocs()` is called in the app pipeline.
2. Security authorization controls do not appear in UI.
Fix: Set `openapi.includeSecurity` to `true` and restart the service.
3. Startup fails when docs are enabled with XML comments.
Fix: Enable XML documentation file generation in the host project so the runtime can load the XML doc file.





