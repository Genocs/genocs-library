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

## Exception Tag Payload Guardrails

Exception enrichment now applies bounded message handling for span status and exception message tags.

- Control characters are sanitized before export.
- Exception message values are capped at `1024` characters.
- Oversized values are truncated with the suffix `...(truncated)`.

This preserves diagnostic signal while avoiding oversized payload pressure in telemetry backends.

## Route Tag Cardinality Policy

`Genocs.Telemetry` prefers route templates for `http.route` (for example, `orders/{orderId}`).

When route metadata is unavailable, the default fallback is a bounded constant:

- `/_unmatched`

This avoids high-cardinality tags from raw request paths.

Optional fallback behavior:

- `telemetry.enableRoutePathFallback`: opt-in to request-path fallback
- `telemetry.normalizeRoutePathFallback`: when enabled (default), path fallback normalizes identifier-like segments (such as numeric IDs and GUIDs)

Example:

```json
{
  "telemetry": {
    "enabled": true,
    "enableRoutePathFallback": true,
    "normalizeRoutePathFallback": true
  }
}
```

## Activity Source Collection Policy

`Genocs.Telemetry` now uses a bounded default source set to reduce unintended trace collection.

Default registered sources:

- `Genocs.Saga`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Messaging.AzureServiceBus`

Future source integration is configuration-driven via `telemetry.activitySources`.
Wildcard collection is opt-in via `telemetry.enableWildcardActivitySources` and should be used only when broad source capture is explicitly required.

Example:

```json
{
  "telemetry": {
    "enabled": true,
    "enableWildcardActivitySources": false,
    "activitySources": [
      "MyCompany.Payments",
      "MyCompany.Inventory"
    ]
  }
}
```

## Host-Mode Behavior

`Genocs.Telemetry` registers tracing and metrics pipelines in both builder flows:

- `WebApplicationBuilder` hosts
- `IServiceCollection` + `IConfiguration` hosts

OpenTelemetry log exporters are not configured by `Genocs.Telemetry` in either host mode.
If you need OTLP or Azure log export, configure it through `Genocs.Logging`.

## OTLP Batch Settings Bounds

`telemetry.exporter` batch processor settings are validated before wiring exporters.

- `maxQueueSize`: supported range `512-65536`, fallback `2048`
- `scheduledDelayMilliseconds`: supported range `100-60000`, fallback `5000`
- `exporterTimeoutMilliseconds`: supported range `1000-120000`, fallback `30000`
- `maxExportBatchSize`: supported range `1-1024`, fallback `512`

If `maxExportBatchSize` resolves to a value greater than `maxQueueSize`, it is reduced to a safe fallback and a warning is emitted.

For high-throughput workloads, prefer increasing `maxQueueSize` first, then tune `maxExportBatchSize` and timeouts incrementally while observing exporter backpressure and dropped-span metrics.

## Deterministic Log Export Ownership

`Genocs.Telemetry` does not configure OpenTelemetry log exporters.

- Use `Genocs.Telemetry` for traces and metrics.
- Use `Genocs.Logging` for log export ownership (OTLP, Azure Application Insights, Seq, Loki, Elasticsearch, file, console).
- Do not split log export between both packages.

This avoids duplicate log ingestion when both packages target the same backend.

## Exporter Conflict Guidance (Azure + OTLP)

`Genocs.Telemetry` allows multiple exporters per signal. This is valid, but should be intentional because it increases export volume and backend cost.

Recommended profiles:

- OTLP-only profile: use OTLP as the single backend for traces/metrics.
- Azure-only profile: use Azure Monitor as the single backend for traces/metrics.
- Dual-export profile: use OTLP and Azure together only for migration windows, side-by-side validation, or temporary failover.

Signal-level guidance:

- For normal production, prefer one exporter per signal.
- If dual export is enabled for traces or metrics, define a time-bound reason and owner.
- Keep logs out of `Genocs.Telemetry`; log export remains owned by `Genocs.Logging`.

Overlap warning when `Genocs.Logging` is enabled:

- If `Genocs.Logging` exports logs to OTLP or Azure, do not add any telemetry-side log export assumptions in runbooks or config templates.
- Treat OTLP/Azure in `telemetry` as trace/metric routing only.

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

Jaeger ingestion is supported via OTLP collector endpoints. `Genocs.Telemetry` uses `OpenTelemetry.Exporter.OpenTelemetryProtocol` for this flow and does not require a direct Jaeger exporter package.

Use one of the Jaeger collector OTLP endpoints:

- gRPC: `http://localhost:4317` with `"protocol": "Grpc"`
- HTTP/protobuf: `http://localhost:4318` with `"protocol": "HttpProtobuf"`

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

### Temporary dual export (OTLP + Azure for traces and metrics)

Use this only for controlled migration or backend comparison windows.

```json
{
  "logger": {
    "otlpEndpoint": "http://localhost:4317",
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
      "enableMetrics": true
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

When this profile is active, expect duplicate traces and metrics across backends by design.

## Minimal-Overhead Profile (Exporters Disabled)

When trace exporters are effectively disabled, `Genocs.Telemetry` keeps instrumentation registration deterministic while reducing enrichment overhead:

- ASP.NET Core and HttpClient custom exception enrichment delegates are skipped.
- Exception recording for tracing instrumentation is disabled.
- SQL statement scrubbing processor is not added in no-export tracing mode.

Use this profile when instrumentation is kept on for compatibility, but trace export is intentionally disabled.

```json
{
  "telemetry": {
    "enabled": true,
    "exporter": {
      "enabled": false,
      "enableTracing": false,
      "enableMetrics": false
    },
    "console": {
      "enabled": false,
      "enableTracing": false,
      "enableMetrics": false
    },
    "azure": {
      "enabled": false,
      "enableTracing": false,
      "enableMetrics": false,
      "connectionString": ""
    }
  }
}
```

## Prometheus Plugin

Prometheus is exposed as a built-in plug-in on top of the OpenTelemetry MeterProvider. The
formerly-separate `Genocs.Metrics` package is no longer required — installing
`Genocs.Telemetry` and enabling the `telemetry.prometheus` sub-section is enough.

When enabled, `AddTelemetry()`:

- Adds the `OpenTelemetry.Exporter.Prometheus.AspNetCore` exporter to the meter provider.
- Registers the auth-gating middleware used by `UsePrometheus()`.

Configuration:

```json
{
  "telemetry": {
    "enabled": true,
    "prometheus": {
      "enabled": true,
      "endpoint": "/metrics",
      "apiKey": null,
      "allowedHosts": []
    }
  }
}
```

Settings:

- `enabled`: master switch for the Prometheus plug-in. Defaults to `false`.
- `endpoint`: scraping path. Defaults to `/metrics`. Always normalized to start with `/`.
- `apiKey`: optional API key. When set, callers must pass `?apiKey=<value>` on the scraping endpoint.
- `allowedHosts`: optional list of allowed hosts (or `x-forwarded-for` values). When set, only matching callers may scrape. If both `apiKey` and `allowedHosts` are empty, the endpoint is open to any caller.

Pipeline wiring:

```csharp
using Genocs.Telemetry;

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Auth-gate middleware (no-op when telemetry.prometheus.enabled is false).
app.UsePrometheus();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    // Map the Prometheus scraping endpoint (no-op when telemetry.prometheus.enabled is false).
    endpoints.MapPrometheus();
});
```

`UsePrometheus()` and `MapPrometheus()` are safe to call unconditionally: when the plug-in is
disabled, both calls are no-ops.

## Main Entry Points

- `AddTelemetry`
- `UsePrometheus` (extension on `IApplicationBuilder`)
- `MapPrometheus` (extension on `IEndpointRouteBuilder`)

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
