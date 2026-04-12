using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Telemetry.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Genocs.Telemetry;

public static class OpenTelemetryExtensions
{
    private const int OtlpDefaultMaxQueueSize = 2048;
    private const int OtlpDefaultScheduledDelayMilliseconds = 5000;
    private const int OtlpDefaultExporterTimeoutMilliseconds = 30000;
    private const int OtlpDefaultMaxExportBatchSize = 512;

    private const int OtlpMinMaxQueueSize = 512;
    private const int OtlpMaxMaxQueueSize = 65536;
    private const int OtlpMinScheduledDelayMilliseconds = 100;
    private const int OtlpMaxScheduledDelayMilliseconds = 60000;
    private const int OtlpMinExporterTimeoutMilliseconds = 1000;
    private const int OtlpMaxExporterTimeoutMilliseconds = 120000;
    private const int OtlpMinMaxExportBatchSize = 1;
    private const int OtlpMaxMaxExportBatchSize = 1024;

    private static readonly string[] DefaultTracingSources =
    [
        "Genocs.Saga",
        "Genocs.Messaging.RabbitMQ",
        "Genocs.Messaging.AzureServiceBus"
    ];

    /// <summary>
    /// Adds OpenTelemetry services to the Genocs application.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The updated Genocs builder.</returns>
    public static IGenocsBuilder AddTelemetry(this IGenocsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        AppOptions appOptions = builder.GetOptions<AppOptions>(AppOptions.Position);
        if (appOptions is null || string.IsNullOrWhiteSpace(appOptions.Service))
        {
            return builder;
        }

        TelemetryOptions telemetryOptions = builder.GetOptions<TelemetryOptions>(TelemetryOptions.Position);
        if (telemetryOptions is null || !telemetryOptions.Enabled)
        {
            return builder;
        }

        bool hasWebApplicationBuilder = builder.WebApplicationBuilder is not null;
        Trace.TraceInformation($"Genocs.Telemetry configures OpenTelemetry traces and metrics only. OpenTelemetry log export is not wired by this package and remains owned by Genocs.Logging. Host mode: {(hasWebApplicationBuilder ? "WebApplicationBuilder" : "ServiceCollection-only")}.");

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: appOptions.Service)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector())
            .WithMetrics(metrics => ConfigureMetrics(metrics, telemetryOptions))
            .WithTracing(tracing => ConfigureTracing(tracing, telemetryOptions));

        return builder;
    }

    private static void ConfigureMetrics(MeterProviderBuilder metrics, TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(options);

        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault())
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation();

        if (TryGetEnabledExporter(options, "metrics", out OtlpExportOptions? exporterOptions, out Uri? endpoint) && exporterOptions.EnableMetrics)
        {
            metrics.AddOtlpExporter(otlpOptions => ApplyExporterOptions(otlpOptions, exporterOptions, endpoint));
        }

        if (options.Console?.Enabled == true && options.Console.EnableMetrics)
        {
            metrics.AddConsoleExporter();
        }

        if (options.Azure?.Enabled == true && options.Azure.EnableMetrics && !string.IsNullOrWhiteSpace(options.Azure.ConnectionString))
        {
            metrics.AddAzureMonitorMetricExporter(azure => azure.ConnectionString = options.Azure.ConnectionString);
        }
    }

    private static void ConfigureTracing(TracerProviderBuilder tracing, TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(tracing);
        ArgumentNullException.ThrowIfNull(options);

        bool enableSqlClientTracing = IsSqlClientTracingEnabled(options);
        bool scrubSqlStatementText = ShouldScrubSqlStatementText(options);

        tracing
            .AddAspNetCoreInstrumentation(aspNetCore =>
            {
                aspNetCore.RecordException = true;
                aspNetCore.EnrichWithHttpRequest = EnrichIncomingRequestActivity;
                aspNetCore.EnrichWithHttpResponse = EnrichIncomingResponseActivity;
                aspNetCore.EnrichWithException = EnrichExceptionActivity;
            })
            .AddHttpClientInstrumentation(httpClient =>
            {
                httpClient.RecordException = true;
                httpClient.EnrichWithException = EnrichExceptionActivity;
            });

        if (enableSqlClientTracing)
        {
            tracing.AddSqlClientInstrumentation(sqlClient =>
            {
                sqlClient.RecordException = true;
            });
        }

        if (scrubSqlStatementText)
        {
            tracing.AddProcessor(new StripSqlStatementTextProcessor());
        }

        if (options.MongoDB?.Enabled == true && options.MongoDB.EnableTracing)
        {
            tracing.AddMongoDBInstrumentation();
        }

        foreach (string source in GetTracingActivitySources(options))
        {
            tracing.AddSource(source);
        }

        if (TryGetEnabledExporter(options, "tracing", out OtlpExportOptions? exporterOptions, out Uri? endpoint) && exporterOptions.EnableTracing)
        {
            tracing.AddOtlpExporter(otlpOptions => ApplyExporterOptions(otlpOptions, exporterOptions, endpoint));
        }

        if (options.Console?.Enabled == true && options.Console.EnableTracing)
        {
            tracing.AddConsoleExporter();
        }

        if (options.Azure?.Enabled == true && options.Azure.EnableTracing && !string.IsNullOrWhiteSpace(options.Azure.ConnectionString))
        {
            tracing.AddAzureMonitorTraceExporter(azure => azure.ConnectionString = options.Azure.ConnectionString);
        }
    }

    internal static bool IsSqlClientTracingEnabled(TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.SqlClient?.Enabled != false;
    }

    internal static bool ShouldScrubSqlStatementText(TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return IsSqlClientTracingEnabled(options) && options.SqlClient?.EnableStatementText != true;
    }

    internal static IReadOnlyCollection<string> GetTracingActivitySources(TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string source in DefaultTracingSources)
        {
            sources.Add(source);
        }

        if (options.ActivitySources is not null)
        {
            foreach (string? source in options.ActivitySources)
            {
                if (!string.IsNullOrWhiteSpace(source))
                {
                    sources.Add(source.Trim());
                }
            }
        }

        if (options.EnableWildcardActivitySources)
        {
            sources.Add("*");
            Trace.TraceWarning("telemetry.enableWildcardActivitySources is enabled. This may increase unintended trace collection and cardinality.");
        }

        return sources.ToArray();
    }

    private static bool TryGetEnabledExporter(
        TelemetryOptions options,
        string signal,
        [NotNullWhen(true)] out OtlpExportOptions? exporterOptions,
        [NotNullWhen(true)] out Uri? endpoint)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(signal);

        exporterOptions = options.Exporter;
        endpoint = null;

        if (exporterOptions?.Enabled != true)
        {
            return false;
        }

        if (!TryParseOtlpEndpoint(exporterOptions.OtlpEndpoint, out endpoint))
        {
            Trace.TraceWarning($"Skipping OpenTelemetry {signal} exporter registration because telemetry.exporter.otlpEndpoint is invalid: '{exporterOptions.OtlpEndpoint ?? "<null>"}'.");
            return false;
        }

        return true;
    }

    internal static bool TryParseOtlpEndpoint(string? endpoint, [NotNullWhen(true)] out Uri? endpointUri)
    {
        endpointUri = null;

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return false;
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? parsedUri))
        {
            return false;
        }

        if (parsedUri.Scheme is not ("http" or "https"))
        {
            return false;
        }

        endpointUri = parsedUri;
        return true;
    }

    private static void ApplyExporterOptions(OtlpExporterOptions otlpOptions, OtlpExportOptions exporterOptions, Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(otlpOptions);
        ArgumentNullException.ThrowIfNull(exporterOptions);
        ArgumentNullException.ThrowIfNull(endpoint);

        (int maxQueueSize, int scheduledDelayMilliseconds, int exporterTimeoutMilliseconds, int maxExportBatchSize) =
            NormalizeOtlpBatchSettings(exporterOptions);

        otlpOptions.Endpoint = endpoint;

        if (Enum.TryParse<OtlpExportProtocol>(exporterOptions.Protocol, true, out OtlpExportProtocol protocol))
        {
            otlpOptions.Protocol = protocol;
        }

        if (Enum.TryParse<ExportProcessorType>(exporterOptions.ProcessorType, true, out ExportProcessorType processorType))
        {
            otlpOptions.ExportProcessorType = processorType;
        }

        otlpOptions.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
        {
            MaxQueueSize = maxQueueSize,
            ScheduledDelayMilliseconds = scheduledDelayMilliseconds,
            ExporterTimeoutMilliseconds = exporterTimeoutMilliseconds,
            MaxExportBatchSize = maxExportBatchSize
        };
    }

    internal static (int MaxQueueSize, int ScheduledDelayMilliseconds, int ExporterTimeoutMilliseconds, int MaxExportBatchSize)
        NormalizeOtlpBatchSettings(OtlpExportOptions exporterOptions)
    {
        ArgumentNullException.ThrowIfNull(exporterOptions);

        int maxQueueSize = NormalizeExporterBatchSetting(
            settingName: nameof(OtlpExportOptions.MaxQueueSize),
            configuredValue: exporterOptions.MaxQueueSize,
            minimum: OtlpMinMaxQueueSize,
            maximum: OtlpMaxMaxQueueSize,
            fallbackValue: OtlpDefaultMaxQueueSize);

        int scheduledDelayMilliseconds = NormalizeExporterBatchSetting(
            settingName: nameof(OtlpExportOptions.ScheduledDelayMilliseconds),
            configuredValue: exporterOptions.ScheduledDelayMilliseconds,
            minimum: OtlpMinScheduledDelayMilliseconds,
            maximum: OtlpMaxScheduledDelayMilliseconds,
            fallbackValue: OtlpDefaultScheduledDelayMilliseconds);

        int exporterTimeoutMilliseconds = NormalizeExporterBatchSetting(
            settingName: nameof(OtlpExportOptions.ExporterTimeoutMilliseconds),
            configuredValue: exporterOptions.ExporterTimeoutMilliseconds,
            minimum: OtlpMinExporterTimeoutMilliseconds,
            maximum: OtlpMaxExporterTimeoutMilliseconds,
            fallbackValue: OtlpDefaultExporterTimeoutMilliseconds);

        int maxExportBatchSize = NormalizeExporterBatchSetting(
            settingName: nameof(OtlpExportOptions.MaxExportBatchSize),
            configuredValue: exporterOptions.MaxExportBatchSize,
            minimum: OtlpMinMaxExportBatchSize,
            maximum: OtlpMaxMaxExportBatchSize,
            fallbackValue: OtlpDefaultMaxExportBatchSize);

        if (maxExportBatchSize > maxQueueSize)
        {
            int safeFallbackBatchSize = Math.Min(OtlpDefaultMaxExportBatchSize, maxQueueSize);
            Trace.TraceWarning(
                $"telemetry.exporter.{nameof(OtlpExportOptions.MaxExportBatchSize)} ({maxExportBatchSize}) cannot be greater than telemetry.exporter.{nameof(OtlpExportOptions.MaxQueueSize)} ({maxQueueSize}). Using fallback value {safeFallbackBatchSize}.");
            maxExportBatchSize = safeFallbackBatchSize;
        }

        return (maxQueueSize, scheduledDelayMilliseconds, exporterTimeoutMilliseconds, maxExportBatchSize);
    }

    private static int NormalizeExporterBatchSetting(string settingName, int configuredValue, int minimum, int maximum, int fallbackValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingName);

        if (configuredValue >= minimum && configuredValue <= maximum)
        {
            return configuredValue;
        }

        Trace.TraceWarning($"telemetry.exporter.{settingName} value '{configuredValue}' is outside supported range [{minimum}, {maximum}]. Using fallback value {fallbackValue}.");
        return fallbackValue;
    }

    private static void EnrichExceptionActivity(Activity activity, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(exception);

        // Persist key exception details as span attributes to simplify querying in downstream backends.
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("error.type", exception.GetType().FullName);
        activity.SetTag("error.message", exception.Message);
        activity.SetTag("exception.source", exception.Source);
        activity.SetTag("exception.hresult", exception.HResult);
        activity.SetTag("exception.target_site", exception.TargetSite?.Name);

        if (exception.InnerException is not null)
        {
            activity.SetTag("exception.inner.type", exception.InnerException.GetType().FullName);
            activity.SetTag("exception.inner.message", exception.InnerException.Message);
        }
    }

    private static void EnrichIncomingRequestActivity(Activity activity, HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(request);

        activity.SetTag("http.request_id", request.HttpContext.TraceIdentifier);

        if (TryGetCorrelationId(request.Headers, out string? correlationId) && !string.IsNullOrWhiteSpace(correlationId))
        {
            activity.SetTag("correlation.id", correlationId);
        }

        string? userId = request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? request.HttpContext.User.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            activity.SetTag("enduser.id", userId);
        }

        SetRouteTag(activity, request.HttpContext);
    }

    private static void EnrichIncomingResponseActivity(Activity activity, HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(response);

        // Route data can be unavailable at request start and become available later in the pipeline.
        SetRouteTag(activity, response.HttpContext);
    }

    private static void SetRouteTag(Activity activity, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(context);

        string route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern?.RawText ?? string.Empty;
        if (string.IsNullOrWhiteSpace(route))
        {
            route = context.Request.Path.Value ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(route))
        {
            activity.SetTag("http.route", route);
        }
    }

    private static bool TryGetCorrelationId(IHeaderDictionary headers, [NotNullWhen(true)] out string? correlationId)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (TryGetHeaderValue(headers, "x-correlation-id", out correlationId))
        {
            return true;
        }

        if (TryGetHeaderValue(headers, "x-request-id", out correlationId))
        {
            return true;
        }

        if (TryGetHeaderValue(headers, "correlation-id", out correlationId))
        {
            return true;
        }

        correlationId = null;
        return false;
    }

    private static bool TryGetHeaderValue(IHeaderDictionary headers, string headerName, [NotNullWhen(true)] out string? value)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        if (headers.TryGetValue(headerName, out var headerValues))
        {
            string parsedValue = headerValues.ToString();
            if (!string.IsNullOrWhiteSpace(parsedValue))
            {
                value = parsedValue;
                return true;
            }
        }

        value = null;
        return false;
    }

    private sealed class StripSqlStatementTextProcessor : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            ArgumentNullException.ThrowIfNull(activity);

            // SqlClient currently emits SQL text by default; scrub it unless explicitly enabled.
            if (activity.GetTagItem("db.system.name") is null && activity.GetTagItem("db.system") is null)
            {
                return;
            }

            activity.SetTag("db.query.text", (object?)null);
            activity.SetTag("db.statement", (object?)null);
        }
    }
}
