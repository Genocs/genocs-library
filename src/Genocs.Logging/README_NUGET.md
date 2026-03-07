# Genocs.Logging

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Logging abstractions and extensions for Genocs applications. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Logging
```

## Getting Started

Use this package to wire structured logging, CQRS handler logging behaviors, and correlation-aware middleware.

## Main Entry Points

- `UseLogging`
- `AddCorrelationContextLogging`
- `AddCommandHandlersLogging`
- `AddEventHandlersLogging`
- `MapLogLevelHandler`
- `UseCorrelationContextLogging`

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

## Non-Overlapping Scenarios

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
