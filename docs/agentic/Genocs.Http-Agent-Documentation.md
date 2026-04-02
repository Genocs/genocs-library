# Genocs.Http Agent Reference

## Agent Operating Mode

- Assume `Genocs.Http` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, interfaces, and option types as the only safe API surface.
- Generate outbound HTTP client registration and consumption code only. Do not invent service discovery, RestEase, or resilience features that are not implemented in this package.
- If package composition is unclear, ask whether `Genocs.Core` is already installed because `AddHttpClient(...)` extends `IGenocsBuilder`.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Http` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Outbound HTTP client abstraction and registration layer for Genocs applications |
| Main value | Typed `IHttpClient` registration, retry-enabled request execution, pluggable serialization, optional correlation header propagation, optional request-URL masking in logs |
| Requires | `Genocs.Core` for `IGenocsBuilder` integration |

## What This Package Is For

Use `Genocs.Http` when you need to:

- register a typed outbound HTTP client through `IGenocsBuilder`
- send GET, POST, PUT, PATCH, and DELETE requests through a single abstraction
- choose between exception-oriented and result-oriented response handling
- customize JSON serialization and deserialization behavior
- propagate correlation headers on outbound calls
- mask sensitive URL fragments in HTTP client logs

## What This Package Does Not Do By Itself

Do not assume `Genocs.Http` can:

- provide service discovery or load balancing behavior from `httpClient.type` or `httpClient.services`
- register named downstream clients per service automatically
- interpret a `restEase` section
- apply circuit breakers, timeouts, or fallback policies beyond retry logic
- preserve non-success responses in the plain `GetAsync<T>` or `PostAsync<T>` methods
- add correlation values unless factories and header names are available
- log or mask request bodies

## Safe Default Mental Model

Treat `Genocs.Http` as four things:

1. A builder extension that registers one typed `IHttpClient` backed by `HttpClientFactory`
2. A retrying wrapper around `HttpClient` for common HTTP verbs
3. A serializer abstraction with a default `System.Text.Json` implementation
4. An optional logging filter that masks configured URL parts when request masking is enabled

If a user asks for discovery, advanced resiliency, or generated API clients, ask which companion package or external library should provide that behavior.

## Fast Start Recipes

### Recipe 1: Minimal Registration

Use this when the application needs a single outbound HTTP abstraction.

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs()
    .AddHttpClient();

genocs.Build();

var app = builder.Build();
app.Run();
```

Effect:
- binds `HttpClientOptions` from the `httpClient` section
- registers `IHttpClient` with the typed implementation `GenocsHttpClient`
- registers the default `SystemTextJsonHttpClientSerializer`
- adds empty correlation factories if no custom factories are already registered

### Recipe 2: Call A JSON API And Preserve Status Codes

Use this when the caller needs both the deserialized payload and the raw `HttpResponseMessage`.

```csharp
using Genocs.Http;

public sealed class CatalogGateway(IHttpClient httpClient)
{
    public async Task<HttpResult<ProductDto>> GetProductAsync(Guid id, CancellationToken cancellationToken)
        => await httpClient.GetResultAsync<ProductDto>($"catalog/products/{id}", cancellationToken: cancellationToken);
}

public sealed record ProductDto(Guid Id, string Name);
```

Use `*ResultAsync` methods when non-success responses should not be turned into exceptions by the public verb helpers.

### Recipe 3: Customize Serialization

Use this when the downstream API requires non-default JSON settings.

```csharp
using System.Text.Json;
using Genocs.Core.Builders;
using Genocs.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpClientSerializer>(
    new SystemTextJsonHttpClientSerializer(
        new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        }));

IGenocsBuilder genocs = builder.AddGenocs()
    .AddHttpClient();

genocs.Build();
```

This works because `GenocsHttpClient` accepts an `IHttpClientSerializer` dependency, but note that `AddHttpClient()` also registers a default serializer as a singleton.

### Recipe 4: Add Correlation Headers And URL Masking

Use this when outbound calls must include correlation metadata and logs must hide sensitive URL fragments.

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICorrelationIdFactory, MyCorrelationIdFactory>();
builder.Services.AddSingleton<ICorrelationContextFactory, MyCorrelationContextFactory>();

IGenocsBuilder genocs = builder.AddGenocs()
    .AddHttpClient(maskedRequestUrlParts: ["token", "password"]);

genocs.Build();
```

This only masks URL fragments in the HTTP client logging pipeline. It does not redact request bodies or arbitrary log properties.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddHttpClient(...)` | Register Genocs outbound HTTP support | Registers one typed `IHttpClient` and binds `HttpClientOptions` from configuration | Assuming it registers multiple downstream clients automatically |
| `RemoveHttpClient()` | Remove the typed `IHttpClient` mapping from the internal registry | Workaround for a typed-client mapping issue | Treating it as normal lifecycle management |
| `IHttpClient` | Send outbound HTTP requests | Supports raw response, typed result, and `HttpResult<T>` methods | Assuming all methods preserve non-success responses |
| `HttpResult<T>` | Keep a typed payload with the raw response | `HasResult` is false when deserialization did not produce a payload | Assuming it throws on non-success status codes |
| `IHttpClientSerializer` | Replace request and response serialization behavior | Used for both payload serialization and stream deserialization | Forgetting to register a serializer compatible with the API contract |
| `SystemTextJsonHttpClientSerializer` | Use default JSON serialization | Uses camelCase, case-insensitive property matching, numeric strings, and camelCase enum text | Assuming it matches every external API by default |
| `ICorrelationContextFactory` | Provide the outbound correlation-context header value | Empty fallback returns `null`-like default string behavior | Assuming correlation data exists without a custom implementation |
| `ICorrelationIdFactory` | Provide the outbound correlation ID header value | Empty fallback returns null | Assuming a correlation ID will always be sent |

## Request And Response Semantics

### Success Handling

- `GetAsync<T>`, `PostAsync<T>`, `PutAsync<T>`, `PatchAsync<T>`, and `DeleteAsync<T>` return `default` when the response is not successful.
- `GetResultAsync<T>` and the other `*ResultAsync<T>` methods return `HttpResult<T>` even when the status code is not successful.
- `SendAsync(HttpRequestMessage)` retries and returns the raw `HttpResponseMessage`.

### Exception Handling

- Public verb methods that internally call the protected `SendAsync(string, Method, ...)` throw when the final response status is not successful.
- `SendAsync<T>(HttpRequestMessage, ...)` throws `HttpRequestException` for non-success responses so Polly retries can run.
- Retry logic is exception-driven. If no exception is thrown, Polly does not retry.

### URI Handling

- Relative-looking URIs are prefixed with `http://` automatically.
- Fully qualified URIs that already start with `http` are used as provided.

That means a call such as `GetAsync("orders/api/items")` becomes `http://orders/api/items`.

## Configuration Ownership

`Genocs.Http` reads the `httpClient` section into `HttpClientOptions`.

```json
{
  "httpClient": {
    "enabled": true,
    "type": "consul",
    "retries": 3,
    "services": {
      "orders": "http://orders-service"
    },
    "removeCharsetFromContentType": true,
    "correlationContextHeader": "x-correlation-context",
    "correlationIdHeader": "x-correlation-id",
    "requestMasking": {
      "enabled": true,
      "urlParts": ["token", "password"],
      "maskTemplate": "*****"
    }
  }
}
```

What this package actively uses:

- `retries`
- `removeCharsetFromContentType`
- `correlationContextHeader`
- `correlationIdHeader`
- `requestMasking.enabled`
- `requestMasking.urlParts`
- `requestMasking.maskTemplate`

What this package defines but does not actively implement by itself:

- `enabled`
- `type`
- `services`

Treat those latter fields as package-shared configuration shape unless another package explicitly documents runtime behavior for them.

## Public Capability Map

### Registration And Integration

- `AddHttpClient(...)`
- `RemoveHttpClient()`
- `HttpClientOptions`

### Outbound Request APIs

- `IHttpClient.GetAsync(...)`
- `IHttpClient.PostAsync(...)`
- `IHttpClient.PutAsync(...)`
- `IHttpClient.PatchAsync(...)`
- `IHttpClient.DeleteAsync(...)`
- `IHttpClient.SendAsync(...)`
- `IHttpClient.SendResultAsync(...)`

### Serialization

- `IHttpClientSerializer`
- `SystemTextJsonHttpClientSerializer`

### Response Wrapping

- `HttpResult<T>`

### Correlation Extension Points

- `ICorrelationContextFactory`
- `ICorrelationIdFactory`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `httpClient.type` enables service discovery or load balancing.
2. Do not assume `httpClient.services` rewrites host names or resolves logical services.
3. Do not assume non-success responses are preserved unless you use `*ResultAsync<T>` or raw `HttpResponseMessage` methods.
4. Do not assume retries cover every failure mode; they run on exceptions only.
5. Do not assume request masking redacts bodies, headers, or arbitrary structured log properties.
6. Do not assume correlation headers are sent unless header names are configured and value factories are meaningful.
7. Do not assume the package defines timeouts, circuit breakers, fallback policies, or hedging.
8. Do not assume multiple named typed clients are registered just because `clientName` is configurable.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Http`, answer these questions:

1. Is `Genocs.Core` already installed so `IGenocsBuilder` is available?
2. Does the caller need exception-based flow or status-aware result handling?
3. Are downstream URIs absolute, or will the implicit `http://` prefixing be acceptable?
4. Does the API require custom serialization behavior?
5. Are correlation headers required, and if so, where do their values come from?
6. Is masking needed only for URL fragments, or is full payload redaction required from another layer?
7. Are additional `HttpClientFactory` policies being configured through the optional `httpClientBuilder` callback?

If any answer is unknown, prefer minimal registration plus `*ResultAsync<T>` methods for safer caller behavior.

## Common Tasks And Safe Responses

### Task: "Register an outbound HTTP client"

Safe response:
- call `builder.AddGenocs().AddHttpClient()`
- keep configuration in `httpClient`
- avoid promising service discovery unless another package is confirmed

### Task: "Call an API and inspect failures"

Safe response:
- use `GetResultAsync<T>` or another `*ResultAsync<T>` overload
- inspect `Response.StatusCode` instead of relying on exceptions alone

### Task: "Customize JSON behavior"

Safe response:
- register a custom `IHttpClientSerializer`
- ensure request and response contracts match the downstream API

### Task: "Add correlation IDs to outbound requests"

Safe response:
- configure `correlationIdHeader` and or `correlationContextHeader`
- register custom factory implementations

### Task: "Hide secrets in logs"

Safe response:
- use `requestMasking` only for URL-part masking
- ask for a separate logging or redaction solution for bodies and headers

## Failure Modes And Troubleshooting

1. Non-success responses are turning into exceptions.
Fix: Use `GetResultAsync<T>` or another `*ResultAsync<T>` overload when the caller must inspect status codes without exception flow.

2. Retries are not happening.
Fix: Ensure `httpClient.retries` is greater than zero and the failure path actually throws an exception.

3. Correlation headers are missing.
Fix: Configure the header names and register non-empty implementations of `ICorrelationContextFactory` or `ICorrelationIdFactory`.

4. URL masking is enabled but logs still show sensitive data.
Fix: Verify the sensitive values appear in the URL, not in the body or headers, and ensure `requestMasking.urlParts` contains the exact fragments to replace.

5. Requests are going to the wrong scheme.
Fix: Pass fully qualified `https://...` URIs when HTTPS is required because relative-looking inputs are prefixed with `http://`.

6. Response content deserializes to `null` unexpectedly.
Fix: Check the serializer contract, the response body format, and whether the API returned a non-success response that caused a default result.

7. Content type includes an unwanted charset.
Fix: Set `removeCharsetFromContentType` to `true` so JSON payloads omit the charset parameter.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Logging`
- `Genocs.Telemetry`
- `Genocs.WebApi`
- `Genocs.Messaging`

## One-Line Recommendation For Agents

If you only know that `Genocs.Http` is installed, generate one typed outbound client through `AddHttpClient()`, prefer `*ResultAsync<T>` when failure inspection matters, and do not promise discovery, advanced resiliency, or redaction features that this package does not implement.
