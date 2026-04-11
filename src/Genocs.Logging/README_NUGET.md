# Genocs.Logging

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Logging abstractions and extensions for Genocs applications. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Logging
```

## Getting Started

Use this package to wire structured logging, CQRS handler logging behaviors, and correlation-aware middleware.

Supported sink families in this package are Console, File, OTLP (logs), Elasticsearch, Seq, Loki, and Azure Application Insights.
Distributed tracing and metrics export should be handled by Genocs.Telemetry.

## Main Entry Points

- `UseLogging`
- `AddCorrelationContextLogging`
- `AddCommandHandlersLogging`
- `AddEventHandlersLogging`
- `MapLogLevelHandler`
- `UseCorrelationContextLogging`

### CQRS Decorator Assembly Selection

`AddCommandHandlersLogging()` and `AddEventHandlersLogging()` discover handlers in the entry assembly by default.

If handlers are declared in another assembly (for example, separate application or feature projects), pass it explicitly:

```csharp
builder.AddGenocs()
	.AddCommandHandlersLogging(typeof(CreateOrder).Assembly)
	.AddEventHandlersLogging(typeof(OrderCreated).Assembly);
```

Optional payload capture can be enabled via:

```json
{
	"logger": {
		"httpPayload": {
			"enabled": true,
			"captureRequestBody": true,
			"captureResponseBody": false,
			"maxBodyLength": 4096
		}
	}
}
```

### HTTP Payload Capture Policy

- Payload capture is disabled by default and should only be enabled for targeted diagnostics.
- `httpPayload.maxBodyLength` is normalized to a safe bounded range.
	Values less than or equal to `0` are treated as `4096` and values above `16384` are capped.
- Content type matching uses strict media-type semantics (for example `application/json`, `application/*+json`) and ignores invalid patterns.
- Request payload is available in request scope and `http.request.body` activity tag.
- Response payload is captured after request pipeline completion and is exposed only through `http.response.body` activity tag.
- When payload capture is disabled, middleware avoids request/response buffering paths.

### Correlation Baggage Guardrails

- Baggage enrichment is bounded to prevent unbounded scope growth in high-throughput scenarios.
- Maximum baggage items added to log scope per request: `32`.
- Baggage keys are trimmed and capped at `64` characters.
- Baggage values are capped at `256` characters.
- Duplicate baggage keys after normalization are ignored.

## Configuration Notes

- `logger.enabled` defaults to `true`. Set it to `false` to disable sink and enrichment wiring from `Genocs.Logging`.
- `logger.seq.enabled` requires a non-empty `logger.seq.url`.
- `logger.loki.enabled` requires a non-empty `logger.loki.url`.
- `MapLogLevelHandler` is a `POST` endpoint that expects a `level` query-string value (for example `/logging/level?level=Debug`).
- `AddCorrelationContextLogging` only registers middleware; `UseCorrelationContextLogging` must be added to the app pipeline to activate it.

## Non-Overlapping Scenarios

Use one logging exporter ownership path per environment to avoid duplicate log ingestion.

### Scenario 1: Send Logging, Metrics, and Traces to Azure Application Insights

Use OpenTelemetry Azure Monitor exporter for all three signals and keep Serilog Azure/OTLP sinks disabled to avoid duplicates.

```json
{
	"logger": {
		"otlpEndpoint": null,
		"azure": {
			"enabled": false
		}
	},
	"telemetry": {
		"enabled": true,
		"exporter": {
			"enabled": false
		},
		"azure": {
			"enabled": true,
			"enableTracing": true,
			"enableMetrics": true,
			"enableLogging": true,
			"connectionString": "InstrumentationKey=<<key>>;IngestionEndpoint=https://<<region>>.in.applicationinsights.azure.com/"
		},
		"console": {
			"enabled": false,
			"enableTracing": false,
			"enableMetrics": false,
			"enableLogging": false
		}
	}
}
```

### Scenario 2: Send Only Traces to Jaeger (OTLP)

Use OTLP exporter for tracing only, disable OTLP logs/metrics, and keep Azure sinks disabled.

```json
{
	"logger": {
		"otlpEndpoint": null,
		"azure": {
			"enabled": false
		}
	},
	"telemetry": {
		"enabled": true,
		"exporter": {
			"enabled": true,
			"otlpEndpoint": "http://localhost:4317",
			"protocol": "Grpc",
			"enableTracing": true,
			"enableMetrics": false,
			"enableLogging": false
		},
		"azure": {
			"enabled": false,
			"enableTracing": false,
			"enableMetrics": false,
			"enableLogging": false
		},
		"console": {
			"enabled": false,
			"enableTracing": false,
			"enableMetrics": false,
			"enableLogging": false
		}
	}
}
```

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
