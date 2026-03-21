# Genocs.Http Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Http is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Http |
| Project file | [src/Genocs.Http/Genocs.Http.csproj](src/Genocs.Http/Genocs.Http.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | Typed outbound HTTP client abstraction with retries, serializer pluggability, optional correlation headers, and request URL masking in logs |
| Core themes | IHttpClient abstraction, Genocs builder registration, Polly retry policy, JSON serializer abstraction, correlation context propagation, URL masking |

## Use This Package When

- Calling downstream HTTP services through a consistent typed client interface.
- Standardizing retry behavior for transient outbound request failures.
- Returning both payload and raw HttpResponseMessage metadata to callers.
- Injecting correlation context and correlation ID headers into outbound calls.
- Masking sensitive URL fragments in HttpClient request logs.

## Do Not Assume

- A URI without scheme is treated as HTTP and rewritten to start with http://.
- Typed and result methods have different failure behavior: some throw and retry, some return default/null with response details.
- Correlation headers are only sent when header names are configured in httpClient options.

## High-Value Entry Points

### Registration and host integration

- AddHttpClient(this IGenocsBuilder, ...) in [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- RemoveHttpClient(this IGenocsBuilder) in [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- HttpClientOptions in [src/Genocs.Http/Configurations/HttpClientOptions.cs](src/Genocs.Http/Configurations/HttpClientOptions.cs)

### Request execution surface

- IHttpClient in [src/Genocs.Http/IHttpClient.cs](src/Genocs.Http/IHttpClient.cs)
- GenocsHttpClient in [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- HttpResult<T> in [src/Genocs.Http/HttpResult.cs](src/Genocs.Http/HttpResult.cs)

### Serialization extension points

- IHttpClientSerializer in [src/Genocs.Http/IHttpClientSerializer.cs](src/Genocs.Http/IHttpClientSerializer.cs)
- SystemTextJsonHttpClientSerializer in [src/Genocs.Http/SystemTextJsonHttpClientSerializer.cs](src/Genocs.Http/SystemTextJsonHttpClientSerializer.cs)

### Correlation and headers

- ICorrelationContextFactory in [src/Genocs.Http/ICorrelationContextFactory.cs](src/Genocs.Http/ICorrelationContextFactory.cs)
- ICorrelationIdFactory in [src/Genocs.Http/ICorrelationIdFactory.cs](src/Genocs.Http/ICorrelationIdFactory.cs)
- EmptyCorrelationContextFactory in [src/Genocs.Http/EmptyCorrelationContextFactory.cs](src/Genocs.Http/EmptyCorrelationContextFactory.cs)
- EmptyCorrelationIdFactory in [src/Genocs.Http/EmptyCorrelationIdFactory.cs](src/Genocs.Http/EmptyCorrelationIdFactory.cs)

### Logging and masking internals

- GenocsHttpLoggingFilter in [src/Genocs.Http/GenocsHttpLoggingFilter.cs](src/Genocs.Http/GenocsHttpLoggingFilter.cs)
- GenocsLoggingScopeHttpMessageHandler in [src/Genocs.Http/GenocsLoggingScopeHttpMessageHandler.cs](src/Genocs.Http/GenocsLoggingScopeHttpMessageHandler.cs)

### Headers and custom requests

- SetHeaders(IDictionary<string, string>) in [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- SetHeaders(Action<HttpRequestHeaders>) in [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- SendAsync(HttpRequestMessage, ...) in [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- SendResultAsync<T>(HttpRequestMessage, ...) in [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Register typed outbound client | AddHttpClient on IGenocsBuilder | Registers IHttpClient, serializer, options, and fallback correlation factories if missing |
| Override typed client setup | AddHttpClient(..., httpClientBuilder: action) | Use callback to set base address, auth handlers, timeouts, and policies |
| Send GET/POST/PUT/PATCH/DELETE with typed payload | IHttpClient generic methods | Non-success returns default/null; use result variants when response metadata is needed |
| Preserve status code with payload | GetResultAsync, PostResultAsync, PutResultAsync, PatchResultAsync, DeleteResultAsync | Returns HttpResult<T> with HasResult and raw Response |
| Execute raw HttpRequestMessage | SendAsync(HttpRequestMessage) | Retries and throws on failures through Polly policy |
| Swap JSON implementation | Implement IHttpClientSerializer | Pass custom serializer per call or replace service registration globally |
| Add correlation context headers | Configure CorrelationContextHeader and CorrelationIdHeader | Header names must be configured; values come from correlation factories |
| Mask secret URL parts in logs | Enable RequestMasking and set UrlParts | Installs GenocsHttpLoggingFilter replacing default handler filter |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Http
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();

gnxBuilder.AddHttpClient(
    clientName: "downstream",
    sectionName: "httpClient",
    httpClientBuilder: client =>
    {
        client.ConfigureHttpClient(c =>
        {
            c.Timeout = TimeSpan.FromSeconds(30);
        });
    });

gnxBuilder.Build();

var app = builder.Build();

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- AddHttpClient uses builder.TryRegister("http.client"); repeated registrations are ignored.
- GenocsHttpClient applies Polly WaitAndRetryAsync with exponential backoff based on Retries.
- URI strings without http/https scheme are rewritten to http://{uri}.
- SendAsync and internal verb pipeline throw on non-success status codes, which triggers retries.
- Generic convenience methods like GetAsync<T> return default/null when response status is non-success.
- SendResultAsync<T> returns HttpResult<T> with response even when status is non-success.
- RemoveCharsetFromContentType removes charset from application/json payload Content-Type when enabled.
- RequestMasking replaces configured URL parts in logs with MaskTemplate via GenocsLoggingScopeHttpMessageHandler.

## Source-Accurate Capability Map

### Client registration and lifecycle

- Registers typed IHttpClient through Microsoft HttpClientFactory.
- Loads HttpClientOptions from configuration section (default httpClient).
- Registers fallback EmptyCorrelationContextFactory and EmptyCorrelationIdFactory when not already present.
- Provides RemoveHttpClient workaround for typed client mapping registry edge case.

Files:

- [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- [src/Genocs.Http/Configurations/HttpClientOptions.cs](src/Genocs.Http/Configurations/HttpClientOptions.cs)

### HTTP verb and request APIs

- Exposes all common verbs with raw response, typed payload, and typed result wrappers.
- Supports custom HttpRequestMessage execution paths.
- Supports cancellation tokens across all methods.
- Supports setting default headers on typed client.

Files:

- [src/Genocs.Http/IHttpClient.cs](src/Genocs.Http/IHttpClient.cs)
- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/HttpResult.cs](src/Genocs.Http/HttpResult.cs)

### Retry and failure behavior

- Uses Polly retry policy in both raw and typed send paths.
- Retries on exceptions with exponential delay formula 2^retryAttempt seconds.
- Throws on non-success in exception-based paths.
- Preserves response without throwing in SendResultAsync path.

Files:

- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)

### Serialization and content handling

- Abstract serializer contract for request and response bodies.
- Default serializer uses System.Text.Json with camelCase and enum string conversion.
- Optional per-call serializer override for all typed methods.
- JSON payload creation supports optional charset removal.

Files:

- [src/Genocs.Http/IHttpClientSerializer.cs](src/Genocs.Http/IHttpClientSerializer.cs)
- [src/Genocs.Http/SystemTextJsonHttpClientSerializer.cs](src/Genocs.Http/SystemTextJsonHttpClientSerializer.cs)
- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)

### Correlation header propagation

- Defines contracts for building correlation context and correlation ID values.
- Adds configured correlation headers to default request headers at client creation time.
- Provides null-object factory implementations when no concrete factory is registered.

Files:

- [src/Genocs.Http/ICorrelationContextFactory.cs](src/Genocs.Http/ICorrelationContextFactory.cs)
- [src/Genocs.Http/ICorrelationIdFactory.cs](src/Genocs.Http/ICorrelationIdFactory.cs)
- [src/Genocs.Http/EmptyCorrelationContextFactory.cs](src/Genocs.Http/EmptyCorrelationContextFactory.cs)
- [src/Genocs.Http/EmptyCorrelationIdFactory.cs](src/Genocs.Http/EmptyCorrelationIdFactory.cs)

### Request logging and URL masking

- Installs HttpMessageHandlerBuilder filter to add logging scope handler.
- Logs request pipeline start and end with event IDs.
- Masks configured URL fragments before logging.
- Uses configurable mask template with default *****.

Files:

- [src/Genocs.Http/GenocsHttpLoggingFilter.cs](src/Genocs.Http/GenocsHttpLoggingFilter.cs)
- [src/Genocs.Http/GenocsLoggingScopeHttpMessageHandler.cs](src/Genocs.Http/GenocsLoggingScopeHttpMessageHandler.cs)

## Dependencies

From [src/Genocs.Http/Genocs.Http.csproj](src/Genocs.Http/Genocs.Http.csproj):

- Genocs.Core
- Microsoft.Extensions.Http
- Polly

## Related Docs

- NuGet package readme: [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md)
