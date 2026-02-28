using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Core.Exceptions;
using Genocs.Telemetry.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
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
    /// <remarks>
    /// This method adds OpenTelemetry services to the Genocs application.
    /// </remarks>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The updated Genocs builder.</returns>
    public static IGenocsBuilder AddTelemetry(this IGenocsBuilder builder)
    {
        AppOptions appOptions = builder.GetOptions<AppOptions>(AppOptions.Position)
            ?? throw new GenocsException.InvalidConfigurationException("app config section is missing. AddTelemetry requires those configuration.");

        // No OpenTelemetryTracing in case of missing ServiceName
        if (string.IsNullOrWhiteSpace(appOptions.Service))
        {
            return builder;
        }

        TelemetryOptions? openTelemetryOptions = builder.GetOptions<TelemetryOptions>(TelemetryOptions.Position);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName: appOptions.Service)
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector();

            })
            .WithMetrics(metrics =>
            {
                // Setup standard instrumentations
                MeterProviderBuilder provider = metrics.SetResourceBuilder(ResourceBuilder.CreateDefault());

                provider.AddAspNetCoreInstrumentation();
                provider.AddRuntimeInstrumentation();
                provider.AddHttpClientInstrumentation();
                provider.AddOtlpExporter();

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

                // Check for Console config
                if (openTelemetryOptions.Console?.Enabled == true && openTelemetryOptions.Console.EnableMetrics)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
                    // Any OTEL supportable exporter can be used here
                    metrics.AddConsoleExporter();
                }

                // Check for Azure ApplicationInsights config
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

                // Setup MongoDB exporter
                if (openTelemetryOptions.MongoDB?.Enabled == true && openTelemetryOptions.MongoDB.EnableTracing)
                {
                    // you should add MongoDB.Driver.Core.Extensions.OpenTelemetry NuGet package
                    tracing.AddMongoDBInstrumentation();
                }

                // TODO: Add more instrumentations as needed, e.g., Redis, gRPC, etc.
                tracing.AddSource("*");

                /*
    Action<ResourceBuilder> appResourceBuilder =
        resource => resource
            .AddDetector(new ContainerResourceDetector());

                builder.Services.AddTelemetry()
                    .ConfigureResource(appResourceBuilder)
                    .WithTracing(tracerBuilder => tracerBuilder
                        .AddRedisInstrumentation(
                            cartStore.GetConnection(),
                            options => options.SetVerboseDatabaseStatements = true)
                        .AddGrpcClientInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter())

*/
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
}
