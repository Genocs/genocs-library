# Genocs.Logging Agent Reference

## Agent Operating Mode

- Assume `Genocs.Logging` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat the package as a Serilog-based host logging integration package, not a generic logging abstraction.
- Generate logging bootstrap, configuration, middleware registration, and CQRS decorator wiring only when those capabilities are explicitly needed.
- Do not invent custom sinks, tracing emitters, or security filtering behavior that this package does not provide.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Logging` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Serilog-based logging bootstrap and sink orchestration for Genocs hosts |
| Main value | Host logging setup, sink configuration, correlation context middleware, runtime log-level switching, CQRS command and event handler logging decorators |
| Requires | ASP.NET Core host composition and `Genocs.Core` |

## What This Package Is For

Use `Genocs.Logging` when you need to:

- configure Serilog through `builder.Host.UseLogging()`
- emit structured logs to console, file, OTLP, Elasticsearch, Seq, Loki, or Application Insights
- expose a runtime log-level change endpoint through `MapLogLevelHandler()`
- add correlation-context logging middleware to the HTTP pipeline
- add command and event handler logging decorators to a Genocs CQRS setup
- initialize an early bootstrap logger before the host is built

## What This Package Does Not Do By Itself

Do not assume `Genocs.Logging` can:

- provide a generic logging abstraction independent of Serilog
- create traces or metrics by itself
- decorate query handlers
- register CQRS handlers for you
- sanitize secrets or redact payloads automatically
- make MongoDB logging work just because `mongo.enabled` exists
- replace the need for `Genocs.Telemetry` when distributed tracing is required

## Safe Default Mental Model

Treat `Genocs.Logging` as four things:

1. A Serilog host bootstrap layer
2. A set of configuration-bound sink options under `logger`
3. Optional HTTP middleware for correlation baggage and payload capture
4. Optional CQRS logging decorators for commands and events

If a user asks for observability beyond logs, identify whether `Genocs.Telemetry` or another package is also installed.

## Fast Start Recipes

### Recipe 1: Minimal Host Logging

Use this when the application only needs structured host logging.

```csharp
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();

var app = builder.Build();
app.Run();
```

Effect:
- creates a bootstrap logger before the host is fully configured
- configures Serilog through the `logger` and `app` sections
- registers `ILoggingService` for runtime level changes

### Recipe 2: Runtime Log-Level Switching

Use this when operators need to change the global log level without redeploying.

```csharp
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();

var app = builder.Build();
app.MapLogLevelHandler("/logging/level");
app.Run();
```

Use a `POST` request such as `/logging/level?level=Debug`.

### Recipe 3: Correlation Context Logging

Use this when request correlation baggage and optional HTTP payload capture should appear in log scope.

```csharp
using Genocs.Core.Builders;
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);
IGenocsBuilder genocs = builder.AddGenocs()
    .AddCorrelationContextLogging();

builder.Host.UseLogging();
genocs.Build();

var app = builder.Build();
app.UseCorrelationContextLogging();
app.Run();
```

This requires both registration and middleware usage.

### Recipe 4: CQRS Handler Logging

Use this when Genocs command and event handlers should emit structured before or after logs through decorators.

```csharp
using Genocs.Core.Builders;
using Genocs.Logging.CQRS;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs()
    .AddCommandHandlersLogging()
    .AddEventHandlersLogging();

genocs.Build();
```

If you want actual message templates, register an `IMessageToLogTemplateMapper` implementation.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `StaticLogger.EnsureInitialized()` | Capture early startup failures | Creates a bootstrap Serilog logger before host build | Skipping it and losing startup diagnostics |
| `builder.Host.UseLogging()` | Configure Serilog for the host | Reads `logger` and `app` sections and registers `ILoggingService` | Calling it after `builder.Build()` |
| `MapLogLevelHandler()` | Expose runtime level-switch endpoint | Maps a `POST` endpoint that changes the shared `LoggingLevelSwitch` | Sending `GET` requests or forgetting `UseLogging()` |
| `AddCorrelationContextLogging()` | Register correlation middleware and options | Adds `CorrelationContextLoggingMiddleware` and stores `LoggerOptions` | Forgetting to also call `UseCorrelationContextLogging()` |
| `UseCorrelationContextLogging()` | Put correlation middleware into the pipeline | Captures Activity baggage and optional request or response payloads | Using it without registering it first |
| `AddCommandHandlersLogging()` | Decorate command handlers with logging | Decorates handlers found in the target assembly | Assuming it registers handlers themselves |
| `AddEventHandlersLogging()` | Decorate event handlers with logging | Decorates handlers found in the target assembly | Assuming query handlers are covered too |
| `IMessageToLogTemplateMapper` | Supply log templates for CQRS decorator logs | If absent, decorators silently emit no template-based logs | Assuming logs will appear without a mapper |

## Choosing The Right Surface

| Situation | Prefer |
|---|---|
| You only need host logging | `StaticLogger.EnsureInitialized()` plus `builder.Host.UseLogging()` |
| You need to change log verbosity at runtime | `MapLogLevelHandler()` |
| You need request-scope baggage or optional payload capture | `AddCorrelationContextLogging()` plus `UseCorrelationContextLogging()` |
| You need command handler logging | `AddCommandHandlersLogging()` |
| You need event handler logging | `AddEventHandlersLogging()` |
| You need query handler logging | Another solution; this package does not provide it |
| You need traces or metrics | Ask for `Genocs.Telemetry` |

## Public Capability Map

### Host Logging Bootstrap

- `StaticLogger.EnsureInitialized()`
- `UseLogging(Action<HostBuilderContext, LoggerConfiguration>? configure = null, string? loggerSectionName = "logger", string? appSectionName = "app")`
- `ILoggingService`
- `MapLogLevelHandler()`

### Correlation And Request Logging

- `AddCorrelationContextLogging()`
- `UseCorrelationContextLogging()`
- `CorrelationContextLoggingMiddleware`
- `HttpPayloadOptions`

### CQRS Logging

- `AddCommandHandlersLogging()`
- `AddEventHandlersLogging()`
- `IMessageToLogTemplateMapper`
- `HandlerLogTemplate`

### Configuration Types

- `LoggerOptions`
- `ConsoleOptions`
- `LocalFileOptions`
- `ElkOptions`
- `SeqOptions`
- `LokiOptions`
- `AzureOptions`
- `MongoOptions`
- `HttpPayloadOptions`

## Configuration Ownership

`Genocs.Logging` owns the `logger` configuration section.

```json
{
  "logger": {
    "level": "Information",
    "otlpEndpoint": "http://localhost:4317",
    "minimumLevelOverrides": {
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "excludePaths": ["/healthz", "/alive"],
    "excludeProperties": ["RequestBody"],
    "tags": {
      "service": "orders-api"
    },
    "console": {
      "enabled": true,
      "enableStructured": false,
      "enableTracing": false,
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

What this package actively uses:

- `level`
- `otlpEndpoint`
- `minimumLevelOverrides`
- `excludePaths`
- `excludeProperties`
- `tags`
- `console.enabled`
- `console.enableStructured`
- `file.enabled`, `file.path`, `file.interval`
- `elk.*`
- `seq.*`
- `loki.*`
- `azure.*`
- `httpPayload.*`

What exists but is not implemented as a sink here:

- `mongo.enabled`

The package also reads the shared `app` section from `Genocs.Common.AppOptions` for enrichment fields such as application, instance, and version.

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Call `builder.Host.UseLogging()` before the host is built.
2. Do not assume `UseLogging()` is additive with another Serilog bootstrap strategy.
3. Do not enable payload capture by default.
4. Do not promise query-handler logging; only command and event decorators exist.
5. Do not assume `IMessageToLogTemplateMapper` is registered automatically.
6. Do not assume `mongo.enabled` activates a MongoDB sink.
7. Do not assume tracing or metrics are emitted just because console flags mention them.
8. Do not assume runtime level switching affects every deployed instance in a distributed environment.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Logging`, answer these questions:

1. Is the host using ASP.NET Core with `builder.Host` available?
2. Is Serilog already configured somewhere else?
3. Are runtime level changes required, or is static config enough?
4. Is correlation baggage or HTTP payload capture actually needed?
5. Are CQRS command or event handlers already registered in the host?
6. Is there a custom `IMessageToLogTemplateMapper`, or should decorator logging stay template-free?
7. Is distributed tracing required, meaning `Genocs.Telemetry` should also be considered?
8. Which sinks are allowed in the target environment?

If any of these answers are unknown, prefer the minimal host-logging setup.

## Common Tasks And Safe Responses

### Task: "Set up logging"

Safe response:
- call `StaticLogger.EnsureInitialized()`
- call `builder.Host.UseLogging()`
- configure only the required sinks under `logger`

### Task: "Capture correlation IDs and request bodies"

Safe response:
- add and use correlation middleware explicitly
- enable payload capture only for approved content types and debugging scenarios

### Task: "Log CQRS handler activity"

Safe response:
- decorate command and event handlers only
- ask whether a custom `IMessageToLogTemplateMapper` should be registered

### Task: "Support runtime log-level changes"

Safe response:
- map `MapLogLevelHandler()`
- mention that it expects `POST` and changes a shared in-process level switch

### Task: "Add tracing"

Safe response:
- note that this package enriches logs from `Activity.Current`
- ask for `Genocs.Telemetry` if actual tracing export is required

## Failure Modes And Troubleshooting

1. No structured logs appear.
Fix: Ensure `builder.Host.UseLogging()` is called before the host is built and that at least one sink is enabled.

2. Startup exceptions are not logged.
Fix: Call `StaticLogger.EnsureInitialized()` at the top of `Program.cs`.

3. Runtime log-level endpoint returns 400.
Fix: Ensure `UseLogging()` registered `ILoggingService`, send a `POST` request, and include a valid `level` query value.

4. Correlation middleware appears inactive.
Fix: Register it with `AddCorrelationContextLogging()` and add it to the pipeline with `UseCorrelationContextLogging()`.

5. CQRS decorator logs never appear.
Fix: Confirm handlers are already registered, confirm the correct assembly is decorated, and register an `IMessageToLogTemplateMapper` if template-based logs are expected.

6. Request or response bodies are missing from logs.
Fix: Check `httpPayload.enabled`, the capture flags, and whether the response content type matches `allowedContentTypes`.

7. Payload logs are too large or expose sensitive data.
Fix: Disable payload capture, reduce `maxBodyLength`, tighten `allowedContentTypes`, or exclude sensitive properties at the sink level.

8. `mongo.enabled` has no effect.
Fix: This package currently does not configure a MongoDB sink from that option.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Common`
- `Genocs.Telemetry`
- `Genocs.WebApi`
- `Genocs.Messaging`
- `Genocs.Auth`

## One-Line Recommendation For Agents

If you only know that `Genocs.Logging` is installed, generate minimal Serilog host bootstrap first, then add optional correlation middleware and CQRS decorators only when the host already uses Genocs.Core and those behaviors are explicitly required.