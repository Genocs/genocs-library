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
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Genocs.Telemetry;

public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry services to the Genocs application.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The updated Genocs builder.</returns>
    public static IGenocsBuilder AddTelemetry(this IGenocsBuilder builder)
    {
        AppOptions appOptions = builder.GetOptions<AppOptions>(AppOptions.Position);
        if (string.IsNullOrWhiteSpace(appOptions.Service))
        {
            return builder;
        }

        TelemetryOptions telemetryOptions = builder.GetOptions<TelemetryOptions>(TelemetryOptions.Position);
        if (!telemetryOptions.Enabled)
        {
            return builder;
        }

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: appOptions.Service)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector())
            .WithMetrics(metrics => ConfigureMetrics(metrics, telemetryOptions))
            .WithTracing(tracing => ConfigureTracing(tracing, telemetryOptions));

        builder.WebApplicationBuilder?.Logging.AddOpenTelemetry(logging => ConfigureLogging(logging, telemetryOptions));

        return builder;
    }

    private static void ConfigureMetrics(MeterProviderBuilder metrics, TelemetryOptions options)
    {
        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault())
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation();

        if (TryGetEnabledExporter(options, out OtlpExportOptions? exporterOptions) && exporterOptions.EnableMetrics)
        {
            metrics.AddOtlpExporter(otlpOptions => ApplyExporterOptions(otlpOptions, exporterOptions));
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
        bool enableSqlStatementText = options.SqlClient?.EnableStatementText == true;

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
            })
            .AddSqlClientInstrumentation(sqlClient =>
            {
                sqlClient.RecordException = true;
            });

        if (!enableSqlStatementText)
        {
            tracing.AddProcessor(new StripSqlStatementTextProcessor());
        }

        if (options.MongoDB?.Enabled == true && options.MongoDB.EnableTracing)
        {
            tracing.AddMongoDBInstrumentation();
        }

        tracing
            .AddSource("*")
            .AddSource("Genocs.Saga");

        if (TryGetEnabledExporter(options, out OtlpExportOptions? exporterOptions) && exporterOptions.EnableTracing)
        {
            tracing.AddOtlpExporter(otlpOptions => ApplyExporterOptions(otlpOptions, exporterOptions));
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

    private static void ConfigureLogging(OpenTelemetryLoggerOptions logging, TelemetryOptions options)
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.ParseStateValues = true;

        if (TryGetEnabledExporter(options, out OtlpExportOptions? exporterOptions) && exporterOptions.EnableLogging)
        {
            logging.AddOtlpExporter(otlpOptions => ApplyExporterOptions(otlpOptions, exporterOptions));
        }

        if (options.Console?.Enabled == true && options.Console.EnableLogging)
        {
            logging.AddConsoleExporter();
        }

        if (options.Azure?.Enabled == true && options.Azure.EnableLogging && !string.IsNullOrWhiteSpace(options.Azure.ConnectionString))
        {
            logging.AddAzureMonitorLogExporter(azure => azure.ConnectionString = options.Azure.ConnectionString);
        }
    }

    private static bool TryGetEnabledExporter(TelemetryOptions options, [NotNullWhen(true)] out OtlpExportOptions? exporterOptions)
    {
        exporterOptions = options.Exporter;
        return exporterOptions?.Enabled == true && !string.IsNullOrWhiteSpace(exporterOptions.OtlpEndpoint);
    }

    private static void ApplyExporterOptions(OtlpExporterOptions otlpOptions, OtlpExportOptions exporterOptions)
    {
        otlpOptions.Endpoint = new Uri(exporterOptions.OtlpEndpoint!);

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
            MaxQueueSize = exporterOptions.MaxQueueSize,
            ScheduledDelayMilliseconds = exporterOptions.ScheduledDelayMilliseconds,
            ExporterTimeoutMilliseconds = exporterOptions.ExporterTimeoutMilliseconds,
            MaxExportBatchSize = exporterOptions.MaxExportBatchSize
        };
    }

    private static void EnrichExceptionActivity(Activity activity, Exception exception)
    {
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
        // Route data can be unavailable at request start and become available later in the pipeline.
        SetRouteTag(activity, response.HttpContext);
    }

    private static void SetRouteTag(Activity activity, HttpContext context)
    {
        string? route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern?.RawText;
        if (string.IsNullOrWhiteSpace(route))
        {
            route = context.Request.Path.Value;
        }

        if (!string.IsNullOrWhiteSpace(route))
        {
            activity.SetTag("http.route", route);
        }
    }

    private static bool TryGetCorrelationId(IHeaderDictionary headers, [NotNullWhen(true)] out string? correlationId)
    {
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
            // SqlClient currently emits SQL text by default; scrub it unless explicitly enabled.
            if (activity.GetTagItem("db.system.name") is null && activity.GetTagItem("db.system") is null)
            {
                return;
            }

            activity.SetTag("db.query.text", null);
            activity.SetTag("db.statement", null);
        }
    }
}
