# Genocs.WebApi Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request package version and configuration details.

## Purpose

`Genocs.WebApi` provides API bootstrap conventions, endpoint composition helpers, request/response utilities, and optional exception-to-response middleware for ASP.NET Core hosts.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.WebApi` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Web API conventions and endpoint DSL |
| Main APIs | `AddWebApi`, `UseEndpoints`, `AddErrorHandler<T>`, `UseErrorHandler`, `UseAllForwardedHeaders` |

## Install

```bash
dotnet add package Genocs.WebApi
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.WebApi;


IGenocsBuilder gnxBuilder = builder.AddGenocs();
gnxBuilder.AddWebApi();
gnxBuilder.Build();

var app = builder.Build();

app.UseEndpoints(endpoints =>
{
    endpoints.Get("/health", async ctx => await ctx.Response.Ok(new { status = "ok" }));
});

app.Run();
```

## Configuration

`Genocs.WebApi` exposes a minimal `webApi` section through `WebApiOptions`.

```json
{
    "webApi": {
        "bindRequestFromRoute": true
    }
}
```

| Setting | Type | Description |
|---|---|---|
| `bindRequestFromRoute` | `bool` | When enabled, request-binding helpers can hydrate request objects from route values in addition to the request body. |

There is also a `WebApiConfigureOptions` type in the project, but it is an options configurator and not a consumer-facing configuration model. The primary runtime knob exposed by the package today is `bindRequestFromRoute`.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Register WebApi conventions and formatters | `AddWebApi()` |
| Map endpoints with Genocs DSL | `UseEndpoints(...)` |
| Add centralized exception mapping | `AddErrorHandler<T>()` and `UseErrorHandler()` |
| Accept all forwarded headers in reverse-proxy deployments | `UseAllForwardedHeaders()` |

## Behavior Notes / Constraints

- `AddWebApi()` is idempotent for repeated registration attempts.
- Error middleware is not auto-enabled by `AddWebApi()`.
- Handler discovery depends on assemblies loaded into the current AppDomain.

## Public Capability Map

- WebApi module registration through `AddWebApi`.
- Endpoint routing and DSL composition through `UseEndpoints`.
- Error pipeline integration through `AddErrorHandler<T>` and `UseErrorHandler`.
- HttpContext and HttpResponse helper APIs for common request/response flows.

## Dependencies

- `Genocs.Core`
- `Open.Serialization.Json.System`
- `Open.Serialization.Json.Utf8Json`
- ASP.NET Core shared framework (`Microsoft.AspNetCore.App`)

## Troubleshooting

1. Endpoints do not respond after startup.
Fix: Ensure `UseEndpoints(...)` is called after `builder.Build()` and before `app.Run()`.
2. Unhandled exceptions return default responses.
Fix: Register a mapper with `AddErrorHandler<T>()` and add `UseErrorHandler()` to the pipeline.
3. Request DTO binding fails for mixed route/body inputs.
Fix: Verify DTO shape and relevant `webApi` binding options used by your host configuration.
