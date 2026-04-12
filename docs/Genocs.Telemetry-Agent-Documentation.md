# Genocs.Telemetry Agent Reference

## Agent Operating Mode

- Assume `Genocs.Telemetry` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods and configuration types as the only safe API surface.
- Generate OpenTelemetry registration, configuration, and observability guidance only.
- Do not invent middleware, endpoint mapping, Prometheus scraping endpoints, Jaeger-specific registration methods, or custom instrumentation hooks that this package does not expose.
- If package composition is unclear, ask whether `Genocs.Core` is already installed because `AddTelemetry()` extends `IGenocsBuilder`.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Telemetry` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | OpenTelemetry integration for traces and metrics |
| Main value | One builder extension that wires OpenTelemetry resource metadata, ASP.NET Core and HttpClient instrumentation, runtime metrics, SQL client tracing, optional MongoDB tracing, and OTLP, Azure, or console exporters for traces and metrics |
| Requires | `Genocs.Core` and a configured `app.service` value |

## What This Package Is For

Use `Genocs.Telemetry` when you need to:

- register OpenTelemetry from a Genocs host with `AddTelemetry()`
- export traces and metrics to OTLP collectors
- export traces and metrics to Azure Monitor
- emit telemetry to the console during development
- instrument incoming ASP.NET Core requests and outgoing `HttpClient` calls
- instrument runtime metrics such as GC and thread pool data
- instrument SQL client spans
- enrich spans with correlation IDs, route information, user IDs, and exception metadata

## What This Package Does Not Do By Itself

Do not assume `Genocs.Telemetry` can:

- expose a `UseTelemetry()` middleware or any required app-pipeline call
- expose Prometheus scrape endpoints
- configure a Jaeger exporter directly even though the package references Jaeger-related dependencies
- register custom `ActivitySource` names beyond the built-in sources and wildcard listener it already configures
- replace Genocs.Logging log exporter responsibilities
- replace `Genocs.Logging` for bootstrap logging, Seq, file sinks, or Serilog-based host logging
- configure MongoDB metrics or MongoDB logs through `telemetry.mongoDB`; this section is tracing-only

## Safe Default Mental Model

Treat `Genocs.Telemetry` as five things:

1. A single builder extension: `AddTelemetry()`
2. An OpenTelemetry resource and exporter setup layer
3. A package that instruments web requests, outbound HTTP, runtime metrics, and SQL automatically
4. A span-enrichment layer that adds correlation, route, user, and exception tags
5. A package that leaves log export ownership to `Genocs.Logging`

If a user asks for local file logging, Seq logging, or Serilog middleware behavior, identify `Genocs.Logging` as the companion package rather than forcing those concerns into Telemetry.

## Fast Start Recipes

### Recipe 1: Minimal OTLP Setup

Use this when the service should export traces and metrics to an OTLP collector.

```csharp
using Genocs.Core.Builders;
using Genocs.Telemetry;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddTelemetry();

genocs.Build();

var app = builder.Build();
app.Run();
```

Configuration:

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "exporter": {
      "enabled": true,
      "otlpEndpoint": "http://localhost:4317",
      "protocol": "Grpc",
      "processorType": "Batch",
      "enableTracing": true,
      "enableMetrics": true
    }
  }
}
```

Effect:

- configures the OpenTelemetry resource with the service name from `app.service`
- enables ASP.NET Core request traces
- enables outgoing `HttpClient` traces and metrics
- enables runtime metrics
- enables SQL client traces
- exports each enabled signal to OTLP

### Recipe 2: Console Telemetry For Development

Use this when developers need immediate local trace or metric output.

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "console": {
      "enabled": true,
      "enableTracing": true,
      "enableMetrics": true
    }
  }
}
```

Use this in development and diagnostics. Do not treat console export as the primary production observability path.

### Recipe 3: Azure Monitor Export

Use this when traces or metrics should go to Azure Monitor.

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "azure": {
      "enabled": true,
      "connectionString": "InstrumentationKey=...;IngestionEndpoint=https://...;LiveEndpoint=https://...;ApplicationId=...",
      "enableTracing": true,
      "enableMetrics": true
    }
  }
}
```

Important behavior:

- Azure export is added only when `azure.enabled` is `true`
- each signal is controlled independently through `enableTracing` and `enableMetrics`
- an empty Azure connection string disables Azure export even if the section is enabled

### Recipe 4: MongoDB Trace Instrumentation

Use this when MongoDB driver operations should appear as traces.

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "mongoDB": {
      "enabled": true,
      "enableTracing": true
    }
  }
}
```

Important behavior:

- MongoDB support in this package is tracing-only
- `telemetry.mongoDB` supports `enabled` and `enableTracing` only
- the repository sample appsettings files mostly use a separate root `mongodb` section for persistence packages, which does not configure `Genocs.Telemetry` by itself

### Recipe 5: Keep SQL Text Scrubbed

Use this when SQL spans are useful but raw SQL text should not be exported.

```json
{
  "telemetry": {
    "enabled": true,
    "sqlClient": {
      "enableStatementText": false
    }
  }
}
```

Important behavior:

- SQL client instrumentation is added when `telemetry.sqlClient.enabled` is either omitted or set to `true`
- set `telemetry.sqlClient.enabled` to `false` to disable SQL client instrumentation registration
- when `enableStatementText` is `false`, a custom processor removes `db.query.text` and `db.statement` tags before export
- this reduces the risk of leaking sensitive SQL or parameterized business data into observability backends

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddTelemetry()` | Register OpenTelemetry traces and metrics from a Genocs host | Returns immediately if `app.service` is empty or `telemetry.enabled` is `false` | Assuming registration always happens |
| `TelemetryOptions` | Configure the `telemetry` section | Owns exporter, console, Azure, MongoDB, and SQL client sub-options | Assuming every option property is actively used |
| `OtlpExportOptions` | Configure OTLP export | Controls endpoint, protocol, processor type, and per-signal enablement | Assuming OTLP export works without `otlpEndpoint` |
| `ConsoleOptions` | Configure console export | Enables console export per signal | Treating it as a production sink by default |
| `AzureOptions` | Configure Azure Monitor export | Enables Azure export per signal when the connection string exists | Assuming `enabled = true` is enough without a connection string |
| `MongoDbOptions` | Configure MongoDB telemetry behavior | Only `enabled` and `enableTracing` affect runtime behavior today | Assuming metrics or logging are implemented for MongoDB |
| `SqlClientOptions` | Configure SQL telemetry behavior | `enabled` controls SQL tracing registration; `enableStatementText` controls SQL text scrubbing | Assuming logging package settings control SQL tracing |

## Instrumentation Semantics

### Metrics

`AddTelemetry()` configures metrics for:

- ASP.NET Core
- .NET runtime
- `HttpClient`

The package does not add Prometheus export or custom application meters on its own.

### Tracing

`AddTelemetry()` configures tracing for:

- ASP.NET Core
- `HttpClient`
- SQL client
- MongoDB when `telemetry.mongoDB.enabled` and `telemetry.mongoDB.enableTracing` are both `true`

It also listens to:

- all activity sources through `AddSource("*")`
- `Genocs.Saga`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Messaging.AzureServiceBus`

That wildcard listener is broad by design. Do not assume the package limits trace collection to Genocs-only sources.

### Logging

`Genocs.Telemetry` does not configure log exporters.

Practical consequence:

- use `Genocs.Logging` as the single owner for log export wiring
- keep `Genocs.Telemetry` focused on traces, metrics, instrumentation, and enrichment

## Span Enrichment Semantics

### Incoming HTTP Requests

The package enriches request spans with:

- `http.request_id` from `HttpContext.TraceIdentifier`
- `correlation.id` from one of these headers, in order:
  - `x-correlation-id`
  - `x-request-id`
  - `correlation-id`
- `enduser.id` from either:
  - `ClaimTypes.NameIdentifier`
  - `sub`
- `http.route` from route metadata when available, otherwise from the request path

### Incoming HTTP Responses

The package also sets `http.route` again during response enrichment because route metadata may be unavailable at request-start time.

### Exceptions

The package enriches exception activity data with:

- `error.type`
- `error.message`
- `exception.source`
- `exception.hresult`
- `exception.target_site`
- inner-exception type and message when an inner exception exists

It also sets the span status to `Error` with the exception message.

## Exporter Semantics

### OTLP

OTLP export is added only when all of these are true:

- `telemetry.exporter.enabled = true`
- `telemetry.exporter.otlpEndpoint` is not empty
- the specific signal flag is enabled for that exporter

Supported configuration values in the package:

- `protocol`: `Grpc` or `HttpProtobuf`
- `processorType`: `Simple` or `Batch`

Batch settings are applied from:

- `maxQueueSize`
- `scheduledDelayMilliseconds`
- `exporterTimeoutMilliseconds`
- `maxExportBatchSize`

### Console

Console exporters are added independently per signal when:

- `telemetry.console.enabled = true`
- the specific signal flag is enabled

### Azure Monitor

Azure exporters are added independently per signal when:

- `telemetry.azure.enabled = true`
- the specific signal flag is enabled
- `telemetry.azure.connectionString` is not empty

### Multiple Exporters

This package can add more than one exporter for the same signal at the same time.

Examples:

- OTLP traces plus console traces
- Azure traces plus console traces
- OTLP metrics plus Azure metrics

That can be useful intentionally. Do not assume only one exporter may be active.

The real duplication risk is eliminated by keeping log export ownership only in `Genocs.Logging`.

## Configuration Ownership

`Genocs.Telemetry` owns the `telemetry` section.

```json
{
  "telemetry": {
    "enabled": true,
    "sqlClient": {
      "enabled": true,
      "enableStatementText": false
    },
    "mongoDB": {
      "enabled": true,
      "enableTracing": true
    },
    "exporter": {
      "enabled": true,
      "otlpEndpoint": "http://localhost:4317",
      "protocol": "Grpc",
      "processorType": "Batch",
      "enableTracing": true,
      "enableMetrics": true,
      "maxQueueSize": 2048,
      "scheduledDelayMilliseconds": 5000,
      "exporterTimeoutMilliseconds": 30000,
      "maxExportBatchSize": 512
    },
    "console": {
      "enabled": true,
      "enableTracing": true,
      "enableMetrics": true
    },
    "azure": {
      "enabled": false,
      "connectionString": "InstrumentationKey=...;IngestionEndpoint=https://...",
      "enableTracing": false,
      "enableMetrics": false
    }
  }
}
```

What this package actively uses:

- `telemetry.enabled`
- `telemetry.exporter.*`
- `telemetry.console.*`
- `telemetry.azure.*`
- `telemetry.mongoDB.enabled`
- `telemetry.mongoDB.enableTracing`
- `telemetry.sqlClient.enabled`
- `telemetry.sqlClient.enableStatementText`

## Relationship With Genocs.Logging

`Genocs.Telemetry` and `Genocs.Logging` are complementary, not interchangeable.

### What Genocs.Logging Owns

`Genocs.Logging` owns:

- Serilog host bootstrap through `builder.Host.UseLogging()`
- bootstrap logging before the host is built
- file, Seq, ELK, Loki, console, and Serilog-based OTLP or Azure sinks
- correlation logging middleware and CQRS logging decorators

### What Genocs.Telemetry Owns

`Genocs.Telemetry` owns:

- OpenTelemetry traces
- OpenTelemetry metrics
- instrumentation and span enrichment

### Safe Integration Patterns

#### Pattern 1: Logging For Logs, Telemetry For Traces And Metrics

Use this when you want:

- Serilog console, file, or Seq output from `Genocs.Logging`
- traces and metrics from `Genocs.Telemetry`

Safe guidance:

- keep `builder.Host.UseLogging()`
- keep `AddTelemetry()`
- configure OTLP/Azure/Seq/Loki/file/console log outputs only through `Genocs.Logging`

### Correlation Relationship

The packages complement each other here:

- `Genocs.Logging` can enrich logs with correlation and activity context
- `Genocs.Telemetry` extracts incoming correlation headers and writes them as span tags

This means logs and traces can still align well when both packages are configured correctly.

### What Not To Assume

Do not assume:

- `Genocs.Telemetry` replaces bootstrap logging from `StaticLogger.EnsureInitialized()`
- `Genocs.Logging` emits traces or metrics by itself
- both packages should export the same logs to the same OTLP or Azure backend by default

## Public Capability Map

### Registration

- `AddTelemetry()`

### Configuration Types

- `TelemetryOptions`
- `OtlpExportOptions`
- `ConsoleOptions`
- `AzureOptions`
- `MongoDbOptions`
- `SqlClientOptions`

### Built-In Instrumentation And Enrichment

- ASP.NET Core tracing and metrics
- `HttpClient` tracing and metrics
- runtime metrics
- SQL client tracing
- optional MongoDB tracing
- request, response, exception, route, correlation, and user-id span enrichment

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `AddTelemetry()` always registers anything. It no-ops when `app.service` is empty or `telemetry.enabled` is `false`.
2. Do not assume a follow-up `UseTelemetry()` call exists. This package is registration-only.
3. Do not assume Jaeger export is exposed just because the package references Jaeger dependencies.
4. Do not assume SQL tracing is always on. `telemetry.sqlClient.enabled` can disable SQL client instrumentation.
5. Do not assume `telemetry.mongoDB` supports metrics or logging flags; MongoDB configuration is tracing-only.
6. Do not assume `Genocs.Telemetry` exports logs. Log export belongs to `Genocs.Logging`.
7. Do not assume the root `mongodb` or `mongoDb` section configures Telemetry. MongoDB tracing lives under `telemetry.mongoDB`.
8. Do not assume enabling `enableStatementText` is harmless. It can expose SQL text and sensitive data.
9. Do not assume only one telemetry exporter can be enabled. The package allows multiple exporters per signal.
10. Do not split log export ownership between packages; keep it in `Genocs.Logging`.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Telemetry`, answer these questions:

1. Is `app.service` configured with a real service name?
2. Is `telemetry.enabled` actually intended to turn the feature on in this environment?
3. Is log export ownership explicitly assigned to `Genocs.Logging` for this deployment?
4. Is SQL statement text safe to emit in this environment?
5. Is MongoDB tracing actually needed, and is it configured under `telemetry.mongoDB` rather than only under persistence settings?
6. Should exporters be OTLP, Azure, console, or a deliberate combination?

If any answer is unknown, prefer trace and metric export first, keep SQL text scrubbing enabled, and keep log export in `Genocs.Logging`.

## Common Tasks And Safe Responses

### Task: "Enable distributed tracing"

Safe response:

- call `AddTelemetry()` during host registration
- set `app.service`
- enable either OTLP or Azure tracing export under `telemetry`

### Task: "See telemetry locally while developing"

Safe response:

- enable `telemetry.console`
- turn on only the signal types needed for the debugging session

### Task: "Export telemetry to Azure Monitor"

Safe response:

- enable `telemetry.azure`
- provide a valid connection string
- enable only the needed signals

### Task: "Instrument MongoDB"

Safe response:

- configure `telemetry.mongoDB.enabled = true`
- configure `telemetry.mongoDB.enableTracing = true`
- do not rely on the separate persistence `mongodb` section for Telemetry behavior

### Task: "Keep SQL safe"

Safe response:

- leave `telemetry.sqlClient.enableStatementText = false`
- mention explicitly that SQL client tracing still runs without raw SQL text

### Task: "Use Logging and Telemetry together"

Safe response:

- keep `Genocs.Logging` for bootstrap and local sinks
- keep `Genocs.Telemetry` for traces and metrics
- disable overlapping OTLP or Azure log export if duplicate ingestion is not wanted

## Failure Modes And Troubleshooting

1. No telemetry appears anywhere.
Fix: Confirm `app.service` is set and `telemetry.enabled` is `true`. `AddTelemetry()` silently returns otherwise.

2. Traces and metrics work, but OpenTelemetry logs never appear.
Fix: This is expected. `Genocs.Telemetry` no longer exports logs. Configure log exporters in `Genocs.Logging`.

3. MongoDB operations are not traced even though Mongo persistence is enabled.
Fix: Add `telemetry.mongoDB.enabled = true` and `telemetry.mongoDB.enableTracing = true`. The root persistence `mongodb` section is separate.

4. SQL spans appear without raw SQL text.
Fix: This is expected when `telemetry.sqlClient.enableStatementText = false`. Enable it only after a data-exposure review.

5. `telemetry.sqlClient.enabled = false` stops SQL tracing as expected.
Fix: Ensure this flag is set under `telemetry.sqlClient` and that the service was restarted with updated configuration.

6. Azure export is configured but nothing is sent.
Fix: Confirm `telemetry.azure.enabled = true`, the per-signal flags are enabled, and the Azure connection string is non-empty.

7. OTLP export is configured but no data reaches the collector.
Fix: Confirm `telemetry.exporter.enabled = true`, `otlpEndpoint` is correct, and the collector accepts the configured protocol.

8. Log volume looks doubled in OTLP or Azure.
Fix: Ensure only `Genocs.Logging` is exporting logs and remove duplicate sink targets in logger configuration.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Logging`
- `Genocs.WebApi`
- `Genocs.Messaging`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Messaging.AzureServiceBus`
- `Genocs.Saga`

## One-Line Recommendation For Agents

If you only know that `Genocs.Telemetry` is installed, generate `AddTelemetry()` with a valid `app.service`, treat the package as registration-only OpenTelemetry infrastructure for traces and metrics, keep SQL text scrubbing enabled by default, and keep log export ownership in `Genocs.Logging`.