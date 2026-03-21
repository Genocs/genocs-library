# Genocs.WebApi.OpenApi — Agent Reference Documentation

> **Format**: AI-optimized agent reference. Structured for rapid decision-making. All capability claims are
> linked to actual source files. Do not infer capabilities not listed here.

---

## 1. Purpose

`Genocs.WebApi.OpenApi` integrates Swashbuckle/OpenAPI documentation into the Genocs builder pipeline. It
registers Swagger generation, configures the `OpenApiInfo` document (title, version, contact, license,
servers), optionally adds a JWT Bearer security definition, generates custom LangChain-compatible operation
IDs, serves Swagger UI or ReDoc, and bridges the `WebApiEndpointDefinitions` DSL (from `Genocs.WebApi`) into
the generated spec via a custom `IDocumentFilter`.

**Package ID**: `Genocs.WebApi.OpenApi`  
**NuGet config section**: `openapi`

---

## 2. Quick Facts

| Property | Value |
|---|---|
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Config section key | `openapi` (= `OpenApiOptions.Position`) |
| Registration guard | `builder.TryRegister("docs.openapi")` — idempotent |
| Swashbuckle version | 9.x (net8/9), 10.x (net10) |
| UI options | Swagger UI (default) or ReDoc |
| Security definition | Optional Bearer JWT; controlled by `IncludeSecurity` |
| Requires | `Genocs.WebApi` (project ref in Debug, package ref in Release) |
| Source file | [`src/Genocs.WebApi.OpenApi/Extensions.cs`](../src/Genocs.WebApi.OpenApi/Extensions.cs) |

---

## 3. Use When

- You need Swagger UI or ReDoc served from any Genocs-based ASP.NET Core host.
- You want the `IEndpointsBuilder` DSL endpoints (registered via `UseEndpoints`) to appear in the OpenAPI
  spec — the `WebApiDocumentFilter` bridges DSL endpoint metadata into the Swagger doc automatically.
- You want LangChain-compatible custom operation IDs (controller action name as operation ID).
- You need Bearer JWT visually documented in Swagger UI to allow authenticated requests from the UI.
- You need multi-server entries in the Swagger JSON for different deployment environments.

---

## 4. Do Not Assume

- **`AddOpenApiDocs` does NOT enable docs by default.** The `openapi.enabled` flag must be `true`; if absent
  or false the method returns immediately without registering anything.
- **Calling `AddOpenApiDocs` twice is safe** — `TryRegister("docs.openapi")` blocks the second call.
- **`UseOpenApiDocs` is required** separately in the middleware pipeline; `AddOpenApiDocs` only registers
  services.
- **XML comments are loaded unconditionally** when docs are enabled: `<EntryAssembly>.xml` is read from
  `AppContext.BaseDirectory`. Missing the XML file causes a runtime exception — ensure
  `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is set in the host project.
- **The `WebApiDocumentFilter`** only documents routes registered via `WebApiEndpointDefinitions` (the DSL).
  Regular MVC controller routes appear via the standard Swashbuckle discovery flow and are not affected.
- **On net10**, `Microsoft.OpenApi` namespace is used instead of `Microsoft.OpenApi.Models` — this is handled
  via `#if NET10_0_OR_GREATER` inside the package; no action required from library users.
- **The fluent builder overload** (`Func<IOpenApiOptionsBuilder, IOpenApiOptionsBuilder>`) builds an
  `OpenApiOptions` in memory and does not read from `appsettings.json`.

---

## 5. High-Value Entry Points

```
Extensions.cs → AddOpenApiDocs(IGenocsBuilder, string sectionName)
Extensions.cs → AddOpenApiDocs(IGenocsBuilder, Func<IOpenApiOptionsBuilder, IOpenApiOptionsBuilder>)
Extensions.cs → AddOpenApiDocs(IGenocsBuilder, OpenApiOptions settings)     ← internal workhorse
Extensions.cs → UseOpenApiDocs(IApplicationBuilder)
Configurations/OpenApiOptions.cs → OpenApiOptions                           ← config model
Builders/OpenApiOptionsBuilder.cs → IOpenApiOptionsBuilder (fluent)
Filters/WebApiDocumentFilter.cs → IDocumentFilter                           ← bridges DSL to spec
```

---

## 6. Decision Matrix

| Goal | API to use |
|---|---|
| Register all Swagger services from config | `builder.AddOpenApiDocs()` (no args) |
| Register Swagger services via fluent code | `builder.AddOpenApiDocs(b => b.Enable(true).WithName("v1")...)` |
| Activate Swagger middleware in pipeline | `app.UseOpenApiDocs()` |
| Show Bearer JWT auth in Swagger UI | Set `openapi.includeSecurity: true` in config |
| Use ReDoc instead of Swagger UI | Set `openapi.reDocEnabled: true` in config |
| Add multiple server entries | Populate `openapi.servers[]` with `url`/`description` pairs |
| Change Swagger JSON route prefix | Set `openapi.routePrefix` |
| Enable Swagger in specific env only | Gate `AddOpenApiDocs` call with environment check before calling |

---

## 7. Minimal Integration Recipe

### 7.1 appsettings.json

```json
{
  "openapi": {
    "enabled": true,
    "name": "v1",
    "title": "My Service API",
    "version": "1.0",
    "description": "My service description.",
    "routePrefix": "docs",
    "includeSecurity": true,
    "reDocEnabled": false,
    "contactName": "Team",
    "contactEmail": "team@example.com",
    "contactUrl": "https://example.com",
    "licenseName": "MIT",
    "licenseUrl": "https://opensource.org/license/mit/",
    "termsOfService": "https://example.com/terms"
  }
}
```

### 7.2 Program.cs

```csharp
IGenocsBuilder gnxBuilder = builder
    .AddGenocs()
    .AddWebApi()
    .AddOpenApiDocs();   // reads from "openapi" section

gnxBuilder.Build();

var app = builder.Build();

app.UseGenocs()
   .UseEndpoints(...)
   .UseOpenApiDocs();   // serves /docs/v1/swagger.json + Swagger UI at /docs
```

### 7.3 Fluent variant (no appsettings.json)

```csharp
builder.AddOpenApiDocs(b => b
    .Enable(true)
    .WithName("v1")
    .WithTitle("My API")
    .WithVersion("1.0")
    .IncludeSecurity(true));
```

---

## 8. Behavior Notes

- **`AddOpenApiDocs` (the overloaded entry points)** sanitise the `sectionName` parameter — empty/whitespace
  reverts to `OpenApiOptions.Position` (`"openapi"`).
- **`GetOptions<OpenApiOptions>`** returns null if the section is absent from config; the method then returns
  the builder unmodified (no exception).
- **The doc is registered as** `builder.Services.AddSingleton(settings)` — the `OpenApiOptions` instance is
  available from DI.
- **`CustomOperationIds`**: only applies to `ControllerActionDescriptor`-backed actions. For non-controller
  endpoints the fallback returns `null` (default Swashbuckle behaviour). The pattern is `{ActionName}` for
  group `"v1"` and `_{ActionName}` otherwise — aligns with LangChain tool naming conventions.
- **Security requirements on net8/net9**: both `AddSecurityDefinition` and `AddSecurityRequirement` are
  registered. On **net10** only `AddSecurityDefinition` is registered (requirement block is commented out
  pending double-check).
- **ReDoc vs Swagger UI**: `UseOpenApiDocs` checks `options.ReDocEnabled` at runtime; there is no dual
  registration — exactly one UI endpoint is registered.
- **`FormatEmptyRoutePrefix`**: `//` double-slash caused by an empty `routePrefix` is auto-corrected to `/`.

---

## 9. Source-Accurate Capability Map

| Capability | Source Location |
|---|---|
| Builder guard / idempotency | [`Extensions.cs` → `TryRegister("docs.openapi")`](../src/Genocs.WebApi.OpenApi/Extensions.cs) |
| Config model | [`Configurations/OpenApiOptions.cs`](../src/Genocs.WebApi.OpenApi/Configurations/OpenApiOptions.cs) |
| Fluent builder interface | [`Configurations/IOpenApiOptionsBuilder.cs`](../src/Genocs.WebApi.OpenApi/Configurations/IOpenApiOptionsBuilder.cs) |
| Fluent builder implementation | [`Builders/OpenApiOptionsBuilder.cs`](../src/Genocs.WebApi.OpenApi/Builders/OpenApiOptionsBuilder.cs) |
| DSL-to-spec bridge | [`Filters/WebApiDocumentFilter.cs`](../src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs) |
| LangChain operation ID generator | [`Extensions.cs` → `c.CustomOperationIds(...)`](../src/Genocs.WebApi.OpenApi/Extensions.cs) |
| Bearer JWT security definition | [`Extensions.cs` → `settings.IncludeSecurity`](../src/Genocs.WebApi.OpenApi/Extensions.cs) |
| Swagger UI / ReDoc middleware | [`Extensions.cs` → `UseOpenApiDocs`](../src/Genocs.WebApi.OpenApi/Extensions.cs) |
| Multi-server registration | [`Extensions.cs` → `settings.Servers` loop](../src/Genocs.WebApi.OpenApi/Extensions.cs) |
| XML comments inclusion | [`Extensions.cs` → `c.IncludeXmlComments`](../src/Genocs.WebApi.OpenApi/Extensions.cs) |

---

## 10. Dependencies

| Dependency | Role |
|---|---|
| `Genocs.WebApi` | Provides `IGenocsBuilder`, `WebApiEndpointDefinitions`, `IEndpointsBuilder` |
| `Swashbuckle.AspNetCore.SwaggerGen` | Core Swagger generation |
| `Swashbuckle.AspNetCore.SwaggerUI` | Swagger UI serving |
| `Swashbuckle.AspNetCore.ReDoc` | ReDoc serving |
| `Swashbuckle.AspNetCore.Annotations` | `[SwaggerOperation]` attribute support |
| `Microsoft.AspNetCore.App` (framework ref) | ASP.NET Core primitives |

---

## 11. Related Docs

- NuGet package readme: [src/Genocs.WebApi.OpenApi/README_NUGET.md](src/Genocs.WebApi.OpenApi/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.WebApi.OpenApi-Agent-Documentation.md](docs/Genocs.WebApi.OpenApi-Agent-Documentation.md)
- Related: [Genocs.WebApi-Agent-Documentation.md](docs/Genocs.WebApi-Agent-Documentation.md) — endpoint DSL whose `WebApiEndpointDefinitions` is consumed by `WebApiDocumentFilter`
- Related: [Genocs.Core-Agent-Documentation.md](docs/Genocs.Core-Agent-Documentation.md) — `IGenocsBuilder` foundation
