# Genocs.Logging Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Logging is responsible for
- Which APIs to call for specific logging goals
- Where source of truth lives for sinks, options, and decorators
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Logging |
| Project file | [src/Genocs.Logging/Genocs.Logging.csproj](src/Genocs.Logging/Genocs.Logging.csproj) |
| Target frameworks | net8.0, net9.0, net10.0 |
| Primary role | Serilog-based structured logging with multi-sink support, correlation context enrichment, dynamic log level control, and CQRS handler logging decorators |
| Core themes | Serilog pipeline setup, sink configuration (Console, File, ELK, Seq, Loki, Azure App Insights, OTLP), log level switching at runtime, HTTP request/response body capture, CQRS decorator logging |

## Use This Package When

- Bootstrapping Serilog in an ASP.NET Core or Worker host
- Routing structured logs to Elasticsearch, Seq, Grafana/Loki, or Azure Application Insights simultaneously
- Enriching logs with OpenTelemetry span IDs, machine name, process/thread IDs, and custom tags
- Injecting request/response body content into log scopes for debugging HTTP traffic
- Wrapping command or event handlers with automatic before/after/on-error log entries
- Exposing a dynamic log-level change HTTP endpoint without restarting the service

## Do Not Assume

- `UseLogging()` must be called on `IHostBuilder` — calling it on `WebApplicationBuilder.Host` is the ASP.NET Core entry path; calling it on a `HostBuilder` works for Worker services
- Sinks are opt-in via `appsettings.json` — every sink block has an `Enabled` gate; an empty or missing config section leaves all sinks off except what is wired manually
- `StaticLogger.EnsureInitialized()` is a bootstrap-only logger — it is intended for use before `UseLogging()` is called so that startup exceptions are captured; it is not a substitute for configuring the main logger

## High-Value Entry Points

### Host Setup

- `UseLogging(this IHostBuilder, Action<HostBuilderContext, LoggerConfiguration>?, string?, string?)` in [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- `StaticLogger.EnsureInitialized()` in [src/Genocs.Logging/Startup.cs](src/Genocs.Logging/Startup.cs)

### Correlation & Middleware

- `AddCorrelationContextLogging(this IGenocsBuilder)` in [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- `UseCorrelationContextLogging(this IApplicationBuilder)` in [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- `CorrelationContextLoggingMiddleware` in [src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs](src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs)

### Dynamic Log Level

- `MapLogLevelHandler(this IEndpointRouteBuilder, string endpointRoute)` in [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- `ILoggingService.SetLoggingLevel(string)` in [src/Genocs.Logging/LoggingService.cs](src/Genocs.Logging/LoggingService.cs)
- `LoggingService` in [src/Genocs.Logging/LoggingService.cs](src/Genocs.Logging/LoggingService.cs)

### CQRS Decorator Logging

- `AddCommandHandlersLogging(this IGenocsBuilder, Assembly?)` in [src/Genocs.Logging/CQRS/Extensions.cs](src/Genocs.Logging/CQRS/Extensions.cs)
- `AddEventHandlersLogging(this IGenocsBuilder, Assembly?)` in [src/Genocs.Logging/CQRS/Extensions.cs](src/Genocs.Logging/CQRS/Extensions.cs)
- `CommandHandlerLoggingDecorator<TCommand>` in [src/Genocs.Logging/CQRS/Decorators/CommandHandlerLoggingDecorator.cs](src/Genocs.Logging/CQRS/Decorators/CommandHandlerLoggingDecorator.cs)
- `EventHandlerLoggingDecorator<TEvent>` in [src/Genocs.Logging/CQRS/Decorators/EventHandlerLoggingDecorator.cs](src/Genocs.Logging/CQRS/Decorators/EventHandlerLoggingDecorator.cs)
- `IMessageToLogTemplateMapper` in [src/Genocs.Logging/CQRS/IMessageToLogTemplateMapper.cs](src/Genocs.Logging/CQRS/IMessageToLogTemplateMapper.cs)
- `HandlerLogTemplate` in [src/Genocs.Logging/CQRS/HandlerLogTemplate.cs](src/Genocs.Logging/CQRS/HandlerLogTemplate.cs)

### Configuration Options

- `LoggerOptions` (config key: `logger`) in [src/Genocs.Logging/Configurations/LoggerOptions.cs](src/Genocs.Logging/Configurations/LoggerOptions.cs)
- `ConsoleOptions` in [src/Genocs.Logging/Configurations/ConsoleOptions.cs](src/Genocs.Logging/Configurations/ConsoleOptions.cs)
- `LocalFileOptions` in [src/Genocs.Logging/Configurations/LocalFileOptions.cs](src/Genocs.Logging/Configurations/LocalFileOptions.cs)
- `ElkOptions` in [src/Genocs.Logging/Configurations/ElkOptions.cs](src/Genocs.Logging/Configurations/ElkOptions.cs)
- `SeqOptions` in [src/Genocs.Logging/Configurations/SeqOptions.cs](src/Genocs.Logging/Configurations/SeqOptions.cs)
- `LokiOptions` in [src/Genocs.Logging/Configurations/LokiOptions.cs](src/Genocs.Logging/Configurations/LokiOptions.cs)
- `AzureOptions` in [src/Genocs.Logging/Configurations/AzureOptions.cs](src/Genocs.Logging/Configurations/AzureOptions.cs)
- `HttpPayloadOptions` in [src/Genocs.Logging/Configurations/HttpPayloadOptions.cs](src/Genocs.Logging/Configurations/HttpPayloadOptions.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Bootstrap Serilog in a host | `hostBuilder.UseLogging()` | Call before `builder.Build()`; reads `logger` and `app` config sections |
| Capture startup errors before Serilog is ready | `StaticLogger.EnsureInitialized()` | Call at the very top of `Program.cs`, before any Serilog setup |
| Enrich logs with HTTP baggage and request/response bodies | `AddCorrelationContextLogging()` + `UseCorrelationContextLogging()` | Opt-in body capture via `HttpPayloadOptions.CaptureRequestBody` / `CaptureResponseBody` |
| Change log level at runtime without restart | `MapLogLevelHandler(app, "/logging/level")` | Exposes a POST endpoint; body param `level` accepts Serilog level names |
| Add structured log entries around every command | `AddCommandHandlersLogging(gnxBuilder)` | Uses `[Decorator]` + Scrutor to wrap all `ICommandHandler<>` in the assembly |
| Add structured log entries around every event handler | `AddEventHandlersLogging(gnxBuilder)` | Same Scrutor decoration pattern for `IEventHandler<>` |
| Customize log message templates per command/event | Implement `IMessageToLogTemplateMapper` | Return a `HandlerLogTemplate` with `Before`, `After`, and per-exception templates; uses SmartFormat |
| Route logs to Elasticsearch | Set `logger:elk:enabled: true` and `logger:elk:url` | Supports basic auth and configurable index format |
| Route logs to Grafana/Loki | Set `logger:loki:enabled: true` and `logger:loki:url` | Supports optional credentials and batch posting settings |
| Route logs to Azure Application Insights | Set `logger:azure:enabled: true` and `logger:azure:connectionString` | Uses Serilog.Sinks.ApplicationInsights |
| Send logs over OpenTelemetry | Set `logger:otlpEndpoint` | Writes to Serilog.Sinks.OpenTelemetry OTLP exporter |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Logging
```

### Setup in Program.cs

```csharp
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Logging;

// Bootstrap logger for startup diagnostics (before host builds)
StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

// Wire Serilog via IHostBuilder
builder.Host.UseLogging();

// Register correlation context middleware (optional)
IGenocsBuilder gnxBuilder = builder.AddGenocs();
gnxBuilder.AddCorrelationContextLogging();

// If using CQRS decorator logging (optional)
// gnxBuilder.AddCommandHandlersLogging();
// gnxBuilder.AddEventHandlersLogging();

gnxBuilder.Build();

var app = builder.Build();

// Activate correlation context enrichment (optional)
app.UseCorrelationContextLogging();

// Expose dynamic log-level endpoint (optional)
app.MapLogLevelHandler("/logging/level");

app.Run();
```

### appsettings.json

```json
{
  "app": {
    "name": "MyService",
    "service": "my-service",
    "instance": "1",
    "version": "1.0.0"
  },
  "logger": {
    "enabled": true,
    "level": "Information",
    "otlpEndpoint": "",
    "console": { "enabled": true, "enableStructured": false },
    "file": { "enabled": false, "path": "logs/logs.txt", "interval": "Day" },
    "elk": { "enabled": false, "url": "http://localhost:9200", "indexFormat": "my-service-{0:yyyy.MM.dd}" },
    "seq": { "enabled": false, "url": "http://localhost:5341" },
    "loki": { "enabled": false, "url": "http://localhost:3100" },
    "azure": { "enabled": false, "connectionString": "" },
    "httpPayload": { "enabled": false, "captureRequestBody": false, "captureResponseBody": false },
    "minimumLevelOverrides": { "Microsoft": "Warning", "System": "Warning" },
    "excludePaths": ["/health", "/metrics"],
    "tags": { "team": "backend" }
  }
}
```

## Behavior Notes That Affect Agent Decisions

- `UseLogging()` registers `ILoggingService` as a singleton automatically; `MapLogLevelHandler` relies on it — do not forget to call `UseLogging()` if using the dynamic level endpoint
- The minimum log level is managed through a shared `LoggingLevelSwitch` instance; changing the level via `SetLoggingLevel()` affects all sinks simultaneously
- `AddCommandHandlersLogging` / `AddEventHandlersLogging` discover handlers in `Assembly.GetCallingAssembly()` by default; pass an explicit `Assembly` parameter if handlers live in a different project
- Structured console output (JSON) is activated by `console.enableStructured: true` — use plain-text console for local dev, structured JSON for production log aggregation
- Elasticsearch sink uses `AutoRegisterTemplate = true` with v6 format; explicit `indexFormat` override is strongly recommended for production to avoid date-partitioned index name collisions
- `CorrelationContextLoggingMiddleware` reads `Activity.Current.Baggage` (W3C trace baggage) and injects it as log scope properties; ensure `AddCorrelationContextLogging()` is called before `gnxBuilder.Build()`
- HTTP body capture is opt-in per direction (`CaptureRequestBody`, `CaptureResponseBody`); enabling response body capture buffers the response stream and may affect memory allocation under high load
- `HandlerLogTemplate` messages are formatted using **SmartFormat.NET** syntax, allowing property interpolation from the command/event object (e.g., `"Handling order {OrderId}"`)

## Source-Accurate Capability Map

### Host & Serilog Pipeline Setup

- `UseLogging(IHostBuilder, ...)` — main entry point; reads `LoggerOptions` from config, applies all enrichers and sink wiring, registers `ILoggingService`
- `MapOptions(...)` — internal; applies enrichers: `FromLogContext`, `WithProperty`, `WithExceptionDetails`, `WithMachineName`, `WithProcessId`, `WithThreadId`, `WithSpan`, `MinimumLevelOverrides`, `ExcludePaths`, `Tags`
- `Configure(...)` — internal; activates each sink based on its `Enabled` flag; supports Console, File, ELK, Seq, Loki, Azure AI, OTLP

Files:

- [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- [src/Genocs.Logging/Startup.cs](src/Genocs.Logging/Startup.cs)

### Correlation Context Middleware

- `CorrelationContextLoggingMiddleware` — reads W3C Activity baggage and injects into log scope; optionally captures request/response bodies into log scope and as Activity tags
- `AddCorrelationContextLogging(IGenocsBuilder)` — registers middleware as transient DI service
- `UseCorrelationContextLogging(IApplicationBuilder)` — adds middleware to HTTP pipeline

Files:

- [src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs](src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs)
- [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)

### Dynamic Log Level Control

- `ILoggingService` / `LoggingService` — thin service with `SetLoggingLevel(string)` that updates the shared `LoggingLevelSwitch`
- `MapLogLevelHandler(IEndpointRouteBuilder, string)` — maps a POST endpoint; reads `?level=` query param and calls `ILoggingService`
- `LoggingLevelSwitch` — internal Serilog `LoggingLevelSwitch` instance; updated without restarting the host

Files:

- [src/Genocs.Logging/LoggingService.cs](src/Genocs.Logging/LoggingService.cs)
- [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)

### CQRS Handler Logging Decorators

- `AddCommandHandlersLogging(IGenocsBuilder, Assembly?)` — discovers all `ICommandHandler<>` implementations and wraps each in `CommandHandlerLoggingDecorator<TCommand>` using Scrutor's `TryDecorate`
- `AddEventHandlersLogging(IGenocsBuilder, Assembly?)` — same for `IEventHandler<>`
- `CommandHandlerLoggingDecorator<TCommand>` — logs `Before`, `After`, and per-exception messages using `HandlerLogTemplate` formatted via SmartFormat; falls back silently if no template is registered
- `EventHandlerLoggingDecorator<TEvent>` — identical structure for event handlers

Files:

- [src/Genocs.Logging/CQRS/Extensions.cs](src/Genocs.Logging/CQRS/Extensions.cs)
- [src/Genocs.Logging/CQRS/Decorators/CommandHandlerLoggingDecorator.cs](src/Genocs.Logging/CQRS/Decorators/CommandHandlerLoggingDecorator.cs)
- [src/Genocs.Logging/CQRS/Decorators/EventHandlerLoggingDecorator.cs](src/Genocs.Logging/CQRS/Decorators/EventHandlerLoggingDecorator.cs)

### Message-to-Template Mapping

- `IMessageToLogTemplateMapper` — resolves a `HandlerLogTemplate` for a given message instance; return `null` to skip logging
- `HandlerLogTemplate` — `Before`, `After`, and exception-specific template strings; `GetExceptionTemplate(Exception)` selector method
- Default behavior: if `IMessageToLogTemplateMapper` is not registered in DI, the decorator uses an internal no-op mapper and skips log emission

Files:

- [src/Genocs.Logging/CQRS/IMessageToLogTemplateMapper.cs](src/Genocs.Logging/CQRS/IMessageToLogTemplateMapper.cs)
- [src/Genocs.Logging/CQRS/HandlerLogTemplate.cs](src/Genocs.Logging/CQRS/HandlerLogTemplate.cs)

### Sink Configuration Options

- `LoggerOptions` — root config: `Enabled`, `Level`, `OtlpEndpoint`, sub-objects per sink, `Tags`, `MinimumLevelOverrides`, `ExcludePaths`, `ExcludeProperties`
- `ConsoleOptions` — `Enabled`, `EnableStructured` (JSON formatter for structured output)
- `LocalFileOptions` — `Enabled`, `Path`, `Interval` (e.g., `"Day"`, `"Hour"`)
- `ElkOptions` — `Enabled`, `Url`, `BasicAuthEnabled`, `Username`, `Password`, `IndexFormat`
- `SeqOptions` — `Enabled`, `Url`, `ApiKey`
- `LokiOptions` — `Enabled`, `Url`, `LokiUsername`, `LokiPassword`, `BatchPostingLimit`, `QueueLimit`, `Period`
- `AzureOptions` — `Enabled`, `ConnectionString`
- `HttpPayloadOptions` — `Enabled`, `CaptureRequestBody`, `CaptureResponseBody`

Files:

- [src/Genocs.Logging/Configurations/LoggerOptions.cs](src/Genocs.Logging/Configurations/LoggerOptions.cs)
- [src/Genocs.Logging/Configurations/ElkOptions.cs](src/Genocs.Logging/Configurations/ElkOptions.cs)
- [src/Genocs.Logging/Configurations/SeqOptions.cs](src/Genocs.Logging/Configurations/SeqOptions.cs)
- [src/Genocs.Logging/Configurations/LokiOptions.cs](src/Genocs.Logging/Configurations/LokiOptions.cs)
- [src/Genocs.Logging/Configurations/AzureOptions.cs](src/Genocs.Logging/Configurations/AzureOptions.cs)
- [src/Genocs.Logging/Configurations/HttpPayloadOptions.cs](src/Genocs.Logging/Configurations/HttpPayloadOptions.cs)

## Dependencies

From [src/Genocs.Logging/Genocs.Logging.csproj](src/Genocs.Logging/Genocs.Logging.csproj):

- `Genocs.Core` — `IGenocsBuilder`, `GetOptions<T>()` configuration helper
- `Serilog.AspNetCore` — Serilog / ASP.NET Core integration, `UseSerilog()` on `IHostBuilder`
- `Serilog.Sinks.ElasticSearch` — Elasticsearch sink
- `Serilog.Sinks.Seq` — Seq sink
- `Serilog.Sinks.Grafana.Loki` — Grafana/Loki sink
- `Serilog.Sinks.ApplicationInsights` — Azure Application Insights sink
- `Serilog.Sinks.OpenTelemetry` — OTLP exporter sink
- `Serilog.Sinks.File` — rolling file sink
- `Serilog.Sinks.Async` — wraps other sinks in async buffered mode
- `Serilog.Enrichers.Environment` / `.Process` / `.Thread` / `.Span` — standard enrichers
- `Serilog.Exceptions` — `WithExceptionDetails()` enricher
- `Microsoft.ApplicationInsights` — Azure AI telemetry infrastructure
- `SmartFormat.NET` — `Smart.Format(template, command)` in CQRS decorators

## Related Docs

- NuGet package readme: [src/Genocs.Logging/README_NUGET.md](src/Genocs.Logging/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Genocs.Core reference: [docs/Genocs.Core-Agent-Documentation.md](docs/Genocs.Core-Agent-Documentation.md)
- Telemetry notes: [/memories/repo/telemetry.md](/memories/repo/telemetry.md)
