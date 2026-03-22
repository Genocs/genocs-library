# Genocs.Http Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet
- Do not rely on repository source code access
- Prefer stable public APIs and extension methods documented here
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

`Genocs.Http` provides a typed outbound HTTP abstraction with retry behavior, serialization extension points, and optional correlation/masking support.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Http` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Outbound HTTP client abstraction for service-to-service calls |
| Core entry points | `AddHttpClient(...)`, `IHttpClient`, `HttpResult<T>`, `IHttpClientSerializer` |

## Install

```bash
dotnet add package Genocs.Http
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();
gnxBuilder.AddHttpClient(clientName: "downstream", sectionName: "httpClient");
gnxBuilder.Build();

var app = builder.Build();
app.Run();
```

## Configuration

Primary section: `httpClient`

```json
{
	"httpClient": {
		"enabled": true,
		"type": "consul",
		"retries": 3,
		"services": {
			"orders": "http://orders-service",
			"catalog": "http://catalog-service"
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

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Enables the section for consumers that read `HttpClientOptions`. |
| `type` | `string` | Resolution mode for downstream services. Supported values in code are `consul` and `Fabio`. |
| `retries` | `int` | Retry count applied by the Genocs HTTP client abstraction. |
| `services` | `object` | Name-to-address map for downstream service resolution. |
| `removeCharsetFromContentType` | `bool` | Removes charset from JSON content types when sending request bodies. |
| `correlationContextHeader` | `string` | Header name used to propagate correlation-context payloads. |
| `correlationIdHeader` | `string` | Header name used to propagate a correlation ID. |
| `requestMasking.enabled` | `bool` | Enables masking of configured URL fragments in request logs. |
| `requestMasking.urlParts` | `string[]` | URL fragments or segments to mask in logs. |
| `requestMasking.maskTemplate` | `string` | Replacement text used when masking matched URL fragments. |

Optional section used by the RestEase integration: `restEase`

```json
{
	"restEase": {
		"enabled": true,
		"loadBalancer": "fabio",
		"services": [
			{
				"name": "catalog",
				"scheme": "https",
				"host": "catalog.internal",
				"port": 443
			}
		]
	}
}
```

| RestEase Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Enables RestEase-specific client registration. |
| `loadBalancer` | `string` | Load-balancer mode used by RestEase-generated clients. |
| `services[].name` | `string` | Logical service name. |
| `services[].scheme` | `string` | Request scheme such as `http` or `https`. |
| `services[].host` | `string` | Host name used by the generated client. |
| `services[].port` | `int` | Service port. |

## Decision Matrix For Agents

| If you need to... | Use |
|---|---|
| Register typed outbound HTTP capabilities | `AddHttpClient(...)` |
| Send standard typed requests | `IHttpClient` HTTP verb methods |
| Preserve status code and response metadata | `GetResultAsync(...)` and other `*ResultAsync` methods returning `HttpResult<T>` |
| Send a custom `HttpRequestMessage` | `SendAsync(...)` or `SendResultAsync<T>(...)` |
| Replace default JSON serializer | Register a custom `IHttpClientSerializer` |

## Behavior Notes / Constraints

- Exception-oriented paths can throw on non-success HTTP responses; result-oriented methods preserve response metadata.
- Retry execution depends on configured retry count and request failure behavior.
- Correlation headers are applied only when corresponding header names and value providers are configured.

## Public Capability Map

- Registration and lifecycle: `AddHttpClient(...)`, `RemoveHttpClient(...)`
- Outbound operations: `IHttpClient` for GET, POST, PUT, PATCH, DELETE, and custom requests
- Response wrappers: `HttpResult<T>` for status-aware result handling
- Serialization extension points: `IHttpClientSerializer`
- Correlation extension points: correlation context and correlation ID factories

## Dependencies

- `Genocs.Core`
- `Microsoft.Extensions.Http`
- `Polly`

## Troubleshooting

1. Retries are not occurring for failing requests.
Fix: Confirm `httpClient.retries` is greater than zero and calls are executed through the registered `IHttpClient` abstraction.
2. Correlation headers are missing on outbound calls.
Fix: Configure `correlationContextHeader` and `correlationIdHeader`, and register correlation value providers.
3. Sensitive URL segments appear in logs.
Fix: Enable `requestMasking` and configure `urlParts` plus `maskTemplate` for consistent masking.
