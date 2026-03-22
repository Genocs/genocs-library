# Genocs.Logging Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

`Genocs.Logging` adds Serilog-based host logging, sink configuration, optional correlation middleware, runtime log-level switching, and CQRS handler logging decorators.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Logging` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Structured logging bootstrap and sink orchestration |
| Core entry points | `UseLogging`, `MapLogLevelHandler`, `AddCorrelationContextLogging`, `AddCommandHandlersLogging`, `AddEventHandlersLogging` |

## Install

```bash
dotnet add package Genocs.Logging
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();

var app = builder.Build();
app.MapLogLevelHandler("/logging/level");
app.Run();
```

## Configuration

Use the `logger` section.

```json
{
	"logger": {
		"enabled": true,
		"level": "Information",
		"otlpEndpoint": "http://localhost:4317",
		"minimumLevelOverrides": {
			"Microsoft": "Warning",
			"System": "Warning"
		},
		"excludePaths": ["/health", "/metrics"],
		"excludeProperties": ["RequestBody"],
		"tags": {
			"service": "orders-api",
			"team": "platform"
		},
		"console": {
			"enabled": true,
			"enableStructured": false,
			"enableTracing": true,
			"enableMetrics": false
		},
		"file": {
			"enabled": false,
			"path": "logs/app.log",
			"interval": "Day"
		},
		"seq": {
			"enabled": false,
			"url": "http://localhost:5341",
			"apiKey": ""
		},
		"httpPayload": {
			"enabled": false,
			"captureRequestBody": false,
			"captureResponseBody": true,
			"maxBodyLength": 4096,
			"allowedContentTypes": ["application/json"]
		}
	}
}
```

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Enables logging configuration through this section. |
| `level` | `string` | Default minimum Serilog level. |
| `otlpEndpoint` | `string` | OTLP endpoint used by the Serilog OpenTelemetry sink. |
| `minimumLevelOverrides` | `object` | Per-source minimum level overrides. |
| `excludePaths` | `string[]` | Request paths excluded from request logging. |
| `excludeProperties` | `string[]` | Log event properties removed before writing. |
| `tags` | `object` | Arbitrary enrichers added as static properties on each log event. |
| `console.enabled` | `bool` | Enables console sink output. |
| `console.enableStructured` | `bool` | Writes structured JSON instead of plain text when enabled. |
| `console.enableTracing` | `bool` | Enables console tracing output when supported by the host configuration. |
| `console.enableMetrics` | `bool` | Enables console metrics output when supported. |
| `file.enabled` | `bool` | Enables local rolling-file logging. |
| `file.path` | `string` | Local log file path. |
| `file.interval` | `string` | Rolling interval understood by `Serilog.Sinks.File`. |
| `elk.enabled` | `bool` | Enables Elasticsearch sink output. |
| `elk.basicAuthEnabled` | `bool` | Enables HTTP basic auth for Elasticsearch. |
| `elk.url` | `string` | Elasticsearch base URL. |
| `elk.username` | `string` | Elasticsearch user name. |
| `elk.password` | `string` | Elasticsearch password. |
| `elk.indexFormat` | `string` | Index naming pattern. |
| `seq.enabled` | `bool` | Enables Seq sink output. |
| `seq.url` | `string` | Seq server URL. |
| `seq.apiKey` | `string` | Seq API key. |
| `loki.enabled` | `bool` | Enables Loki sink output. |
| `loki.url` | `string` | Loki push endpoint URL. |
| `loki.batchPostingLimit` | `int` | Maximum number of events per batch. |
| `loki.queueLimit` | `int?` | In-memory queue size before backpressure/drop behavior applies. |
| `loki.period` | `TimeSpan?` | Delay between batch flushes. |
| `loki.lokiUsername` | `string` | Optional Loki user name. |
| `loki.lokiPassword` | `string` | Optional Loki password. |
| `azure.enabled` | `bool` | Enables Azure Application Insights sink output. |
| `azure.connectionString` | `string` | Application Insights connection string. |
| `mongo.enabled` | `bool` | Enables MongoDB-oriented logging/tracing integration where supported. |
| `httpPayload.enabled` | `bool` | Enables request/response payload capture for logs and activity tags. |
| `httpPayload.captureRequestBody` | `bool` | Captures request bodies. |
| `httpPayload.captureResponseBody` | `bool` | Captures response bodies. |
| `httpPayload.maxBodyLength` | `int` | Max captured payload size in characters. |
| `httpPayload.allowedContentTypes` | `string[]` | Content types eligible for capture. |

Keep `httpPayload` disabled unless you need deep diagnostics. It increases memory use and can expose sensitive data if you do not pair it with filtering and masking.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Enable host logging | `builder.Host.UseLogging()` |
| Capture startup failures before host is built | `StaticLogger.EnsureInitialized()` |
| Expose runtime level switch endpoint | `app.MapLogLevelHandler("/logging/level")` |
| Add correlation context logging middleware | `AddCorrelationContextLogging()` plus `UseCorrelationContextLogging()` |
| Add CQRS command/event decorator logging | `AddCommandHandlersLogging()` and `AddEventHandlersLogging()` |

## Behavior Notes / Constraints

- Runtime level changes apply through a shared level switch.
- Correlation logging middleware must be both registered and added to the pipeline.
- CQRS decorator registration depends on handler assembly discovery.
- Sink output depends on each configured sink enablement flag.

## Public Capability Map

- Host logging bootstrap through `UseLogging`.
- Runtime log-level control through `MapLogLevelHandler` and `ILoggingService`.
- Correlation middleware registration and pipeline integration.
- CQRS command and event handler decorator logging.

## Dependencies

- `Genocs.Core`
- Serilog packages
- `SmartFormat.NET`

## Troubleshooting

1. Logs only appear in console.
Fix: Enable and configure the target sink under `logger`.
2. Runtime log-level endpoint returns an error response.
Fix: Ensure `builder.Host.UseLogging()` is called before app build and keep `MapLogLevelHandler` mapped.
3. CQRS decorator logs never appear.
Fix: Register command and event logging decorators for the assembly that contains the handlers.
