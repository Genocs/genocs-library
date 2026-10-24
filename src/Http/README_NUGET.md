# Genocs.Http

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Http client abstractions and helpers for Genocs applications. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Http
```

## Getting Started

Use this package to provide the base Http abstraction layer for outbound service calls and client configuration in Genocs services. Registration flows through `Genocs.Core` (`IGenocsBuilder.AddHttpClient(...)`).

## Main Entry Points

- `AddHttpClient` — registers a typed `IHttpClient` (`GenocsHttpClient`), `HttpClientOptions`, and the default `SystemTextJsonHttpClientSerializer`. Use the optional `httpClientBuilder` argument to call `ConfigureHttpClient` (for example to set `HttpClient.BaseAddress` for relative URI strings).

## Configuration contract (`httpClient`)

`AddHttpClient(...)` binds `HttpClientOptions` from the `httpClient` section.

```json
{
  "httpClient": {
    "enabled": true,
    "type": "consul",
    "retries": 3,
    "retryUnsafeHttpMethods": false,
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

Fields actively used by this package runtime:

- `retries`
- `retryUnsafeHttpMethods`
- `removeCharsetFromContentType`
- `correlationContextHeader`
- `correlationIdHeader`
- `requestMasking.enabled`
- `requestMasking.urlParts`
- `requestMasking.maskTemplate`

Fields currently present in the options contract but not implemented by this package runtime:

- `enabled`
- `type`
- `services`

Treat those fields as shared configuration shape unless a companion package documents concrete runtime behavior.

## Response handling

- **String URI, exception-oriented helpers** — `GetAsync`, `PostAsync`, `GetAsync<T>`, `PostAsync<T>`, and similar either return `HttpResponseMessage` after a successful status or **throw** when the status is not successful (typed methods deserialize only after success).
- **String URI, result-oriented helpers** — Methods named `*ResultAsync<T>(string, ...)` (for example `GetResultAsync<T>`, `PostResultAsync<T>`) return `HttpResult<T>` for both success and failure status codes. Inspect `HttpResult<T>.Response` for status and content; they do **not** throw solely because the HTTP status indicates an error. For non-success responses, `Result` is typically the default value for `T`; use `HasResult` only as a hint about a non-null deserialized payload.
- **`HttpRequestMessage` overloads** — `SendResultAsync<T>(HttpRequestMessage, ...)` preserves non-success responses in `HttpResult<T>`. `SendAsync<T>(HttpRequestMessage, ...)` throws when the status is not successful (exception-oriented deserialization path). Request-message overloads send the provided `HttpRequestMessage` once per call and do not internally retry by replaying that instance.
- **Response ownership** — Typed methods that return `T?` consume and dispose transient `HttpResponseMessage` instances internally. Methods that return `HttpResponseMessage` or `HttpResult<T>` transfer response ownership to the caller.

## Retries

- Retries apply only to **transport-level transient failures** during send (for example `HttpRequestException` without a response status, retryable `HttpRequestException` status codes, and `IOException`).
- Retries apply to string-URI helper methods. `HttpRequestMessage` overloads intentionally do not replay the same message instance.
- Cancellation-driven failures are **not retried**. User-triggered cancellation is surfaced immediately as `OperationCanceledException` / `TaskCanceledException`.
- **HTTP error status codes are not retried** by default. Exception-oriented methods still throw on non-success responses, but that happens after the retry boundary.
- **Deserialization failures are not retried**.
- By default, retries are limited to idempotent methods. `POST`, `PUT`, and `PATCH` are excluded unless explicitly enabled.
- Configure retry count via `httpClient.retries` and opt-in write-method retries via `httpClient.retryUnsafeHttpMethods` in `HttpClientOptions`.
- Cancellation tokens are propagated through send, response-stream access, and payload deserialization paths.

## Correlation headers

- Correlation headers are injected in a **per-request** handler, not in `HttpClient.DefaultRequestHeaders`.
- Header values are evaluated for each outbound request through `ICorrelationContextFactory` and `ICorrelationIdFactory`.
- If a caller already sets the configured correlation headers on `HttpRequestMessage`, Genocs.Http preserves those values and does not overwrite them.
- If header names are blank, or factory outputs are null/whitespace, no correlation headers are added.
- This request-scoped model avoids stale correlation values when typed clients are long-lived while still supporting singleton and scoped consumers.

## Request masking

- Request masking applies to **log URI rendering only**. It does not modify the outbound request URI, request body, or headers.
- Masking uses **exact-token replacement** (`StringComparison.Ordinal`) over the URI text rendered for logging (`HttpRequestMessage.RequestUri.OriginalString`).
- All occurrences of each configured token are replaced, including path, query, and fragment segments.
- Masking is deterministic and safe for diagnostics rendering: replacement output is treated as plain log text and is **not** reparsed as a `Uri`.
- Empty or whitespace tokens are ignored.

## Request URIs and `BaseAddress`

- Request URI strings are parsed with `System.Uri` (`UriKind.RelativeOrAbsolute`) after trimming. **Null**, **whitespace-only**, or **unparseable** values throw **`ArgumentException`** with parameter name **`uri`**.
- Pass **absolute** URIs (for example `https://api.contoso.com/v1/items`) when you want a fully qualified destination; they are sent unchanged (no scheme or host rewriting).
- Pass **relative** paths (for example `items/5` or `/items/5`) when the named `HttpClient` has a **`BaseAddress`** set, for example:

  `AddHttpClient(..., httpClientBuilder: b => b.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.contoso.com/v1/")))`

  Resolution follows normal `HttpClient` / `Uri` combination rules. Prefer a **`BaseAddress` that ends with `/`** when you want relative segments to append as subpaths predictably.
- The client does **not** prepend `http://` or guess a scheme for host-like strings. **Upgrade note:** older versions prepended `http://` when the string did not start with `http`; that behavior was removed. Use an absolute URI, or set `BaseAddress` and pass relative paths.
- If you use a **relative** URI without **`BaseAddress`**, `HttpClient` fails when sending (for example `InvalidOperationException`); configure the base address explicitly rather than relying on implicit rewriting.

## Additional examples

### Configure `BaseAddress` for relative request paths

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

IGenocsBuilder genocs = builder.AddGenocs()
  .AddHttpClient(httpClientBuilder: b =>
    b.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://catalog.internal/api/v1/")));
```

### Override the default serializer

```csharp
using System.Text.Json;
using Genocs.Http;

builder.Services.AddSingleton<IHttpClientSerializer>(
  new SystemTextJsonHttpClientSerializer(
    new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
      PropertyNameCaseInsensitive = true
    }));
```

### Add correlation factories and URL masking

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

builder.Services.AddSingleton<ICorrelationIdFactory, MyCorrelationIdFactory>();
builder.Services.AddSingleton<ICorrelationContextFactory, MyCorrelationContextFactory>();

IGenocsBuilder genocs = builder.AddGenocs()
  .AddHttpClient(maskedRequestUrlParts: ["token", "password"]);
```

Masking applies only to HTTP-client URL log rendering, not to payloads or arbitrary log properties.

## Resilience ownership guidance

- Genocs.Http already applies internal retry behavior for string-URI helper methods.
- If you also add `IHttpClientBuilder` resilience handlers (for example retry/timeout/circuit-breaker), evaluate the combined strategy to avoid duplicate retry amplification.
- Keep retry ownership explicit per outbound call path: either rely on Genocs.Http defaults, or move strategy ownership to pipeline handlers and tune Genocs retries conservatively.
- `HttpRequestMessage` overloads are single-send by design; if replay behavior is required, implement replay-safe resilience at the handler/pipeline layer.

## Migration notes (April 2026)

When upgrading from older Genocs.Http behavior, review these consumer-visible changes:

- String `*ResultAsync<T>(...)` methods preserve non-success responses in `HttpResult<T>` instead of throwing solely for status.
- URI inputs follow standard `System.Uri` rules only; there is no implicit `http://` prefixing.
- Retries are transport-focused; non-success HTTP statuses and deserialization failures are not retried by default.
- Retries for `POST`/`PUT`/`PATCH` require explicit opt-in via `httpClient.retryUnsafeHttpMethods=true`.
- `HttpRequestMessage` overloads are single-send and do not replay the same request instance.
- Correlation headers are injected per request and do not overwrite caller-supplied request headers.
- URL masking applies only to logged URI text and does not redact bodies/headers.

For a source-blind upgrade checklist and decision guidance, see [Genocs.Http-Agent-Documentation.md](https://github.com/Genocs/genocs-library/blob/main/docs/Genocs.Http-Agent-Documentation.md).

## Validation

```bash
dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo
dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo
```

## Maintainer quality gates

Genocs.Http applies package-level quality gates in `Genocs.Http.csproj` to reduce contract regressions:

- nullable warnings are treated as errors (`WarningsAsErrors` includes `nullable`)
- .NET analyzer warnings are treated as errors for this package (`CodeAnalysisTreatWarningsAsErrors=true`)

This gate is intentionally package-scoped (not repository-wide) so maintainers can harden outbound HTTP public-surface behavior without forcing unrelated projects to adopt the same baseline immediately.

## Further documentation

- Agent-oriented reference (semantics, recipes, troubleshooting): [Genocs.Http-Agent-Documentation.md](https://github.com/Genocs/genocs-library/blob/main/docs/Genocs.Http-Agent-Documentation.md)
- Implementation backlog and roadmap: [Genocs.Http-Implementation-Backlog.md](https://github.com/Genocs/genocs-library/blob/main/docs/Genocs.Http-Implementation-Backlog.md)

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
