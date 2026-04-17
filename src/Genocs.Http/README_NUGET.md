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

## Request URIs and `BaseAddress`

- Request URI strings are parsed with `System.Uri` (`UriKind.RelativeOrAbsolute`) after trimming. **Null**, **whitespace-only**, or **unparseable** values throw **`ArgumentException`** with parameter name **`uri`**.
- Pass **absolute** URIs (for example `https://api.contoso.com/v1/items`) when you want a fully qualified destination; they are sent unchanged (no scheme or host rewriting).
- Pass **relative** paths (for example `items/5` or `/items/5`) when the named `HttpClient` has a **`BaseAddress`** set, for example:

  `AddHttpClient(..., httpClientBuilder: b => b.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.contoso.com/v1/")))`

  Resolution follows normal `HttpClient` / `Uri` combination rules. Prefer a **`BaseAddress` that ends with `/`** when you want relative segments to append as subpaths predictably.
- The client does **not** prepend `http://` or guess a scheme for host-like strings. **Upgrade note:** older versions prepended `http://` when the string did not start with `http`; that behavior was removed. Use an absolute URI, or set `BaseAddress` and pass relative paths.
- If you use a **relative** URI without **`BaseAddress`**, `HttpClient` fails when sending (for example `InvalidOperationException`); configure the base address explicitly rather than relying on implicit rewriting.

## Further documentation

- Agent-oriented reference (semantics, recipes, troubleshooting): [Genocs.Http-Agent-Documentation.md](https://github.com/Genocs/genocs-library/blob/main/docs/Genocs.Http-Agent-Documentation.md)
- Implementation backlog and roadmap: [Genocs.Http-Implementation-Backlog.md](https://github.com/Genocs/genocs-library/blob/main/docs/Genocs.Http-Implementation-Backlog.md)

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
