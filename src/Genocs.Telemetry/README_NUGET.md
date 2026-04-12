# Genocs.Telemetry

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

OpenTelemetry integration helpers for traces and metrics. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Telemetry
```

## Getting Started

Use this package to configure OpenTelemetry traces and metrics in Genocs services.

Service registration:

```csharp
using Genocs.Telemetry;

genocs.AddTelemetry();
```

Configuration example:

```json
{
  "app": {
    "service": "My Service"
  },
  "telemetry": {
    "enabled": true,
    "sqlClient": {
      "enabled": true,
      "enableStatementText": false
    },
    "exporter": {
      "enabled": true,
      "otlpEndpoint": "http://localhost:4317",
      "protocol": "Grpc",
      "enableTracing": true,
      "enableMetrics": true
    }
  }
}
```

`telemetry.sqlClient.enabled` controls whether SQL client tracing instrumentation is registered.
When omitted, it defaults to `true` for backward compatibility.

`telemetry.sqlClient.enableStatementText` is disabled by default. Enable it only when SQL query text (`db.query.text`/`db.statement`) collection is explicitly required.

If `telemetry.exporter.enabled` is `true` and `telemetry.exporter.otlpEndpoint` is missing or invalid, OTLP exporter registration is skipped and a warning is emitted. Startup remains safe and traces/metrics continue with the remaining configured exporters.

## Deterministic Log Export Ownership

`Genocs.Telemetry` does not configure OpenTelemetry log exporters.

- Use `Genocs.Telemetry` for traces and metrics.
- Use `Genocs.Logging` for log export ownership (OTLP, Azure Application Insights, Seq, Loki, Elasticsearch, file, console).
- Do not split log export between both packages.

This avoids duplicate log ingestion when both packages target the same backend.

## Non-Overlapping Deployment Templates

### Azure Application Insights (all logs from Genocs.Logging)

```json
{
  "logger": {
    "azure": {
      "enabled": true,
      "connectionString": "InstrumentationKey=<<key>>;IngestionEndpoint=https://<<region>>.in.applicationinsights.azure.com/"
    },
    "otlpEndpoint": null
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
      "connectionString": "InstrumentationKey=<<key>>;IngestionEndpoint=https://<<region>>.in.applicationinsights.azure.com/"
    },
    "console": {
      "enabled": false,
      "enableTracing": false,
      "enableMetrics": false
    }
  }
}
```

### Jaeger with OTLP trace-only (logs still owned by Genocs.Logging)

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
      "enableMetrics": false
    },
    "azure": {
      "enabled": false,
      "enableTracing": false,
      "enableMetrics": false
    },
    "console": {
      "enabled": false,
      "enableTracing": false,
      "enableMetrics": false
    }
  }
}
```

## Main Entry Points

- `AddTelemetry`

## Validation

Run this command before publishing telemetry changes to verify analyzer/nullability baseline remains warning-clean:

```bash
dotnet build src/Genocs.Telemetry/Genocs.Telemetry.csproj -c Debug --nologo
```

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
