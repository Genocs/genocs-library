using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Telemetry.Configurations;
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
        tracing
            .AddAspNetCoreInstrumentation(aspNetCore =>
            {
                aspNetCore.RecordException = true;
            })
            .AddHttpClientInstrumentation(httpClient =>
            {
                httpClient.RecordException = true;
            });

        if (options.MongoDB?.Enabled == true && options.MongoDB.EnableTracing)
        {
            tracing.AddMongoDBInstrumentation();
        }

        tracing.AddSource("*");

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
}
