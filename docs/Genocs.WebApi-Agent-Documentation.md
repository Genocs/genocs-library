# Genocs.WebApi Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.WebApi is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.WebApi |
| Project file | [src/Genocs.WebApi/Genocs.WebApi.csproj](src/Genocs.WebApi/Genocs.WebApi.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | ASP.NET Core Web API conventions, endpoint mapping, request binding, and error handling for Genocs applications |
| Core themes | Web API bootstrap, endpoint builder DSL, request/response helpers, exception middleware, JSON serialization |

## Use This Package When

- You need a consistent way to register Web API conventions with Genocs builder.
- You want a compact endpoint DSL for GET/POST/PUT/DELETE with auth/policy wiring.
- You need request DTO binding from body/query/route plus validation in minimal APIs.
- You want centralized exception-to-response mapping middleware.
- You need JSON formatter control and serializer abstraction integration.

## Do Not Assume

- AddWebApi does not add error middleware by itself; call AddErrorHandler<T> and UseErrorHandler explicitly.
- Handler auto-registration only works for assemblies already loaded in AppDomain.
- UseAllForwardedHeaders resets known networks/proxies by default, which changes trust boundaries.

## High-Value Entry Points

### Builder and service registration

- AddWebApi in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- AddErrorHandler<T> in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- WebApiOptions in [src/Genocs.WebApi/Configurations/WebApiOptions.cs](src/Genocs.WebApi/Configurations/WebApiOptions.cs)
- WebApiConfigureOptions in [src/Genocs.WebApi/Configurations/WebApiConfigureOptions.cs](src/Genocs.WebApi/Configurations/WebApiConfigureOptions.cs)

### Pipeline wiring and infrastructure

- UseEndpoints in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- UseErrorHandler in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- UseAllForwardedHeaders in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- ErrorHandlerMiddleware in [src/Genocs.WebApi/Exceptions/ErrorHandlerMiddleware.cs](src/Genocs.WebApi/Exceptions/ErrorHandlerMiddleware.cs)

### Endpoint DSL and metadata

- IEndpointsBuilder in [src/Genocs.WebApi/IEndpointsBuilder.cs](src/Genocs.WebApi/IEndpointsBuilder.cs)
- EndpointsBuilder in [src/Genocs.WebApi/EndpointsBuilder.cs](src/Genocs.WebApi/EndpointsBuilder.cs)
- WebApiEndpointDefinitions in [src/Genocs.WebApi/WebApiEndpointDefinition.cs](src/Genocs.WebApi/WebApiEndpointDefinition.cs)
- WebApiEndpointDefinition models in [src/Genocs.WebApi/WebApiEndpointDefinition.cs](src/Genocs.WebApi/WebApiEndpointDefinition.cs)

### Request dispatch and handlers

- IRequest in [src/Genocs.WebApi/Requests/IRequest.cs](src/Genocs.WebApi/Requests/IRequest.cs)
- IRequestHandler<TRequest, TResult> in [src/Genocs.WebApi/Requests/IRequestHandler.cs](src/Genocs.WebApi/Requests/IRequestHandler.cs)
- IRequestDispatcher in [src/Genocs.WebApi/Requests/IRequestDispatcher.cs](src/Genocs.WebApi/Requests/IRequestDispatcher.cs)
- RequestDispatcher in [src/Genocs.WebApi/Requests/RequestDispatcher.cs](src/Genocs.WebApi/Requests/RequestDispatcher.cs)

### JSON and formatter pipeline

- JsonInputFormatter in [src/Genocs.WebApi/Formatters/JsonInputFormatter.cs](src/Genocs.WebApi/Formatters/JsonInputFormatter.cs)
- JsonOutputFormatter in [src/Genocs.WebApi/Formatters/JsonOutputFormatter.cs](src/Genocs.WebApi/Formatters/JsonOutputFormatter.cs)
- GenocsFormatterResolver in [src/Genocs.WebApi/GenocsFormatterResolver.cs](src/Genocs.WebApi/GenocsFormatterResolver.cs)
- JsonParser in [src/Genocs.WebApi/Parsers/JsonParser.cs](src/Genocs.WebApi/Parsers/JsonParser.cs)

### HTTP context helpers

- ReadJsonAsync<T> in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- ReadQuery<T> in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- DispatchAsync<TRequest, TResult> in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- Response status helpers (Ok/Created/Accepted/NoContent/etc.) in [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Bootstrap Web API conventions | AddWebApi | Registers serializer, MVC core, formatters, request handlers, and defaults |
| Add global exception mapping | AddErrorHandler<T> + UseErrorHandler | AddErrorHandler registers middleware and mapper, UseErrorHandler inserts middleware |
| Map HTTP endpoints with auth | UseEndpoints + IEndpointsBuilder | Supports roles/policies/auth toggle and endpoint metadata capture |
| Bind query-based request model | EndpointsBuilder.Get<T>/Delete<T> + ReadQuery<T> | Reads route and query string into DTO via serializer |
| Bind JSON body request model | EndpointsBuilder.Post<T>/Put<T> + ReadJsonAsync<T> | Handles deserialize, optional route overlay, and validation |
| Dispatch application request handlers | IRequestDispatcher.DispatchAsync | Uses scoped handler resolution per dispatch |
| Return standard HTTP responses | HttpResponse extension helpers | Use Ok/Created/Accepted/NoContent/NotFound/Forbidden for consistency |
| Forward reverse-proxy headers | UseAllForwardedHeaders | Default behavior clears known proxies/networks for broad forwarding |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.WebApi
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.WebApi;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs();

genocs
    .AddWebApi()
    .AddErrorHandler<MyExceptionToResponseMapper>();

genocs.Build();

var app = builder.Build();

app.UseErrorHandler();
app.UseAllForwardedHeaders();
app.UseEndpoints(endpoints =>
{
    endpoints.Get("/health", async ctx => await ctx.Response.Ok(new { status = "ok" }));
});

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- AddWebApi uses TryRegister("webApi"), so repeated calls are ignored after first registration.
- If Newtonsoft serializer is supplied, synchronous IO is enabled for Kestrel (and IIS only on supported target frameworks).
- ReadJsonAsync<T> can bind route values into private backing fields when BindRequestFromRoute is enabled in webApi options.
- ReadJsonAsync<T> performs DataAnnotations validation and returns HTTP 400 with validation payload on failure.
- ErrorHandlerMiddleware writes mapped response as JSON and falls back to HTTP 400 with empty body when mapper returns null.

## Source-Accurate Capability Map

### Web API bootstrap capabilities

- Registers serializer abstraction and HTTP context accessor.
- Configures MVC core with custom JSON input/output formatters.
- Registers IRequestHandler<,> implementations by assembly scanning.
- Registers IRequestDispatcher and a default empty exception mapper.

Files:

- [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- [src/Genocs.WebApi/Configurations/WebApiOptions.cs](src/Genocs.WebApi/Configurations/WebApiOptions.cs)
- [src/Genocs.WebApi/Formatters/JsonInputFormatter.cs](src/Genocs.WebApi/Formatters/JsonInputFormatter.cs)

### Endpoint composition capabilities

- Maps GET/POST/PUT/DELETE endpoints with typed and untyped overloads.
- Applies authorization, role, or policy requirements per endpoint.
- Builds endpoint metadata (method/path/parameters/responses) for downstream usage.
- Supports query-style and body-style request DTO handling paths.

Files:

- [src/Genocs.WebApi/IEndpointsBuilder.cs](src/Genocs.WebApi/IEndpointsBuilder.cs)
- [src/Genocs.WebApi/EndpointsBuilder.cs](src/Genocs.WebApi/EndpointsBuilder.cs)
- [src/Genocs.WebApi/WebApiEndpointDefinition.cs](src/Genocs.WebApi/WebApiEndpointDefinition.cs)

### Request and handler dispatch capabilities

- Defines marker request contract and typed handler interface.
- Dispatches request handlers via DI scope isolation.
- Provides HttpContext-based dispatch helper for endpoint code.
- Keeps request processing abstraction independent from controller inheritance.

Files:

- [src/Genocs.WebApi/Requests/IRequest.cs](src/Genocs.WebApi/Requests/IRequest.cs)
- [src/Genocs.WebApi/Requests/IRequestHandler.cs](src/Genocs.WebApi/Requests/IRequestHandler.cs)
- [src/Genocs.WebApi/Requests/RequestDispatcher.cs](src/Genocs.WebApi/Requests/RequestDispatcher.cs)

### Error handling capabilities

- Catches unhandled exceptions in middleware.
- Maps exceptions via pluggable IExceptionToResponseMapper.
- Writes mapped response payloads with JSON serializer abstraction.
- Logs exceptions with structured logging through ILogger.

Files:

- [src/Genocs.WebApi/Exceptions/ErrorHandlerMiddleware.cs](src/Genocs.WebApi/Exceptions/ErrorHandlerMiddleware.cs)
- [src/Genocs.WebApi/Exceptions/IExceptionToResponseMapper.cs](src/Genocs.WebApi/Exceptions/IExceptionToResponseMapper.cs)
- [src/Genocs.WebApi/Exceptions/ExceptionResponse.cs](src/Genocs.WebApi/Exceptions/ExceptionResponse.cs)

### Serialization and formatting capabilities

- Uses pluggable IJsonSerializer for request and response payloads.
- Implements formatter-based MVC JSON read/write behavior.
- Exposes formatter resolver for Utf8Json scenarios.
- Includes JSON key-value parser utility for config-like payload parsing.

Files:

- [src/Genocs.WebApi/Formatters/JsonInputFormatter.cs](src/Genocs.WebApi/Formatters/JsonInputFormatter.cs)
- [src/Genocs.WebApi/Formatters/JsonOutputFormatter.cs](src/Genocs.WebApi/Formatters/JsonOutputFormatter.cs)
- [src/Genocs.WebApi/GenocsFormatterResolver.cs](src/Genocs.WebApi/GenocsFormatterResolver.cs)
- [src/Genocs.WebApi/Parsers/JsonParser.cs](src/Genocs.WebApi/Parsers/JsonParser.cs)

### HTTP helper capabilities

- Provides response helper methods for common status codes.
- Reads query and route data into typed request objects.
- Supports route argument extraction with conversion.
- Supports object bind helpers for immutable-style request DTOs.

Files:

- [src/Genocs.WebApi/Extensions.cs](src/Genocs.WebApi/Extensions.cs)
- [src/Genocs.WebApi/Helpers/Extensions.cs](src/Genocs.WebApi/Helpers/Extensions.cs)

## Dependencies

From [src/Genocs.WebApi/Genocs.WebApi.csproj](src/Genocs.WebApi/Genocs.WebApi.csproj):

- Genocs.Core
- Open.Serialization.Json.System
- Open.Serialization.Json.Utf8Json
- Microsoft.AspNetCore.App framework reference

## Related Docs

- NuGet package readme: [src/Genocs.WebApi/README_NUGET.md](src/Genocs.WebApi/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.WebApi-Agent-Documentation.md](docs/Genocs.WebApi-Agent-Documentation.md)
