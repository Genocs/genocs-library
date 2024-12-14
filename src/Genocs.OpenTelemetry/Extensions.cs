using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Core.Exceptions;
using Genocs.GnxOpenTelemetry.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Genocs.GnxOpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static IGenocsBuilder AddOpenTelemetry(
                                            this IGenocsBuilder builder)
    {
        AppOptions appOptions = builder.GetOptions<AppOptions>(AppOptions.Position)
            ?? throw new GenocsException.InvalidConfigurationException("app config section is missing. AddOpenTelemetry requires those configuration.");

        // No OpenTelemetryTracing in case of missing ServiceName
        if (string.IsNullOrWhiteSpace(appOptions.Service))
        {
            return builder;
        }

        OpenTelemetryOptions? openTelemetryOptions = builder.GetOptions<OpenTelemetryOptions>(OpenTelemetryOptions.Position);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: appOptions.Service))
            .WithMetrics(metrics =>
            {
                // Setup standard instrumentations
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                // Setup the exporter
                if (openTelemetryOptions.Exporter?.Enabled == true)
                {
                    metrics
                        .AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(openTelemetryOptions.Exporter.OtlpEndpoint!);

                            // Check if Jaeger is enabled
                            if (openTelemetryOptions.Exporter?.Enabled == true)
                            {
                                // Parse enum
                                otlpOptions.Protocol = Enum.Parse<OpenTelemetry.Exporter.OtlpExportProtocol>(openTelemetryOptions.Exporter.Protocol);
                                otlpOptions.ExportProcessorType = Enum.Parse<ExportProcessorType>(openTelemetryOptions.Exporter.ProcessorType);

                                // Check if Batch Exporter before setting options
                                otlpOptions.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                                {
                                    MaxQueueSize = openTelemetryOptions.Exporter.MaxQueueSize,
                                    ScheduledDelayMilliseconds = openTelemetryOptions.Exporter.ScheduledDelayMilliseconds,
                                    ExporterTimeoutMilliseconds = openTelemetryOptions.Exporter.ExporterTimeoutMilliseconds,
                                    MaxExportBatchSize = openTelemetryOptions.Exporter.MaxExportBatchSize
                                };
                            }
                        });
                }

                // Setup Console exporter
                if (openTelemetryOptions.Console?.Enabled == true && openTelemetryOptions.Console.EnableMetrics)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
                    metrics.AddConsoleExporter();
                }

                // Setup Azure exporter
                if (openTelemetryOptions.Azure?.Enabled == true && openTelemetryOptions.Azure.EnableMetrics)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
                    metrics.AddAzureMonitorMetricExporter(o =>
                    {
                        o.ConnectionString = openTelemetryOptions.Azure.ConnectionString;
                    });
                }
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                // Setup the exporter
                if (openTelemetryOptions.Exporter?.Enabled == true)
                {
                    tracing
                        .AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(openTelemetryOptions.Exporter.OtlpEndpoint!);

                            // Check if Jaeger is enabled
                            if (openTelemetryOptions.Exporter?.Enabled == true)
                            {
                                // Parse enum
                                otlpOptions.Protocol = Enum.Parse<OpenTelemetry.Exporter.OtlpExportProtocol>(openTelemetryOptions.Exporter.Protocol);
                                otlpOptions.ExportProcessorType = Enum.Parse<ExportProcessorType>(openTelemetryOptions.Exporter.ProcessorType);

                                // Check if Batch Exporter before setting options
                                otlpOptions.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                                {
                                    MaxQueueSize = openTelemetryOptions.Exporter.MaxQueueSize,
                                    ScheduledDelayMilliseconds = openTelemetryOptions.Exporter.ScheduledDelayMilliseconds,
                                    ExporterTimeoutMilliseconds = openTelemetryOptions.Exporter.ExporterTimeoutMilliseconds,
                                    MaxExportBatchSize = openTelemetryOptions.Exporter.MaxExportBatchSize
                                };
                            }
                        });
                }

                // Setup Console exporter
                if (openTelemetryOptions.Console?.Enabled == true && openTelemetryOptions.Console.EnableTracing)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
                    tracing.AddConsoleExporter();
                }

                // Setup Azure exporter
                if (openTelemetryOptions.Azure?.Enabled == true && openTelemetryOptions.Azure.EnableTracing)
                {
                    // you should add Azure.Monitor.OpenTelemetry.Exporter NuGet package
                    tracing.AddAzureMonitorTraceExporter(o =>
                    {
                        o.ConnectionString = openTelemetryOptions.Azure.ConnectionString;
                    });
                }
            });

        // Add the OpenTelemetry logging provider
        builder.WebApplicationBuilder?.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;

            // Setup the exporter
            if (openTelemetryOptions.Exporter?.Enabled == true)
            {
                logging.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(openTelemetryOptions.Exporter.OtlpEndpoint!);

                    // Check if Jaeger is enabled
                    if (openTelemetryOptions.Exporter?.Enabled == true)
                    {
                        // Parse enum
                        otlpOptions.Protocol = Enum.Parse<OpenTelemetry.Exporter.OtlpExportProtocol>(openTelemetryOptions.Exporter.Protocol);
                        otlpOptions.ExportProcessorType = Enum.Parse<ExportProcessorType>(openTelemetryOptions.Exporter.ProcessorType);

                        // Check if Batch Exporter before setting options
                        otlpOptions.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                        {
                            MaxQueueSize = openTelemetryOptions.Exporter.MaxQueueSize,
                            ScheduledDelayMilliseconds = openTelemetryOptions.Exporter.ScheduledDelayMilliseconds,
                            ExporterTimeoutMilliseconds = openTelemetryOptions.Exporter.ExporterTimeoutMilliseconds,
                            MaxExportBatchSize = openTelemetryOptions.Exporter.MaxExportBatchSize
                        };
                    }
                });
            }

            // Setup Console exporter
            if (openTelemetryOptions.Console?.Enabled == true && openTelemetryOptions.Console.EnableLogging)
            {
                // you should add OpenTelemetry.Exporter.Console NuGet package
                logging.AddConsoleExporter();
            }

            // Setup Azure exporter
            if (openTelemetryOptions.Azure?.Enabled == true && openTelemetryOptions.Azure.EnableLogging)
            {
                // you should add Azure.Monitor.OpenTelemetry.Exporter NuGet package
                logging.AddAzureMonitorLogExporter(o =>
                {
                    o.ConnectionString = openTelemetryOptions.Azure.ConnectionString;
                });
            }
        });

        return builder;
    }

    /*
    private static void SetupJaegerExporter(OpenTelemetry.Exporter.OtlpExporterOptions provider)
    {
        provider.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(jaegerOptions.Endpoint);

            // Parse enum
            o.Protocol = Enum.Parse<OpenTelemetry.Exporter.OtlpExportProtocol>(jaegerOptions.Protocol);
            o.ExportProcessorType = Enum.Parse<ExportProcessorType>(jaegerOptions.ProcessorType);

            // Check if Batch Exporter before setting options
            o.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
            {
                MaxQueueSize = jaegerOptions.MaxQueueSize,
                ScheduledDelayMilliseconds = jaegerOptions.ScheduledDelayMilliseconds,
                ExporterTimeoutMilliseconds = jaegerOptions.ExporterTimeoutMilliseconds,
                MaxExportBatchSize = jaegerOptions.MaxExportBatchSize
            };
        });
    }
    */

}
