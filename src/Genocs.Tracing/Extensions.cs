using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Logging.Configurations;
using Genocs.Tracing.Jaeger.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using static Genocs.Core.Exceptions.GenocsException;

namespace Genocs.Tracing;

/// <summary>
/// The Open Telemetry extensions.
/// </summary>
public static class Extensions
{

    /// <summary>
    /// It allows to insert OpenTelemetry into the build pipeline.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The Genocs builder you can use for chain.</returns>
    public static IGenocsBuilder AddOpenTelemetry(this IGenocsBuilder builder)
    {
        AppOptions appOptions = builder.GetOptions<AppOptions>(AppOptions.Position)
            ?? throw new InvalidConfigurationException("app config section is missing. AddOpenTelemetry requires those configuration.");

        // No OpenTelemetryTracing in case of missing ServiceName
        if (string.IsNullOrWhiteSpace(appOptions.Service))
        {
            return builder;
        }

        LoggerOptions loggerOptions = builder.GetOptions<LoggerOptions>(LoggerOptions.Position);

        if (loggerOptions is null)
        {
            return builder;
        }

        // OpenTelemetry Logging
        builder.WebApplicationBuilder?.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var services = builder.Services;

        // Set Custom Open telemetry
        services.AddOpenTelemetry()
            .WithTracing((TracerProviderBuilder x) =>
            {
                TracerProviderBuilder provider = x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                                            .AddService(
                                                                        serviceName: appOptions.Service,
                                                                        serviceVersion: appOptions.Version,
                                                                        serviceInstanceId: appOptions.Instance)
                                                            .AddTelemetrySdk()
                                                            .AddEnvironmentVariableDetector())
                                                            .AddAspNetCoreInstrumentation()
                                                            .AddHttpClientInstrumentation()
                                                        .AddSource("*");

                // No OpenTelemetryTracing in case of missing LoggerSettings
                if (loggerOptions.Mongo?.Enabled == true)
                {
                    // Check for MongoDB config
                    provider.AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");
                }

                // Check for Console config
                if (loggerOptions.Console?.Enabled == true && loggerOptions.Console.EnableTracing)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
                    // Any OTEL supportable exporter can be used here
                    provider.AddConsoleExporter();
                }

                // Check for Azure ApplicationInsights config
                if (loggerOptions.Azure?.Enabled == true && loggerOptions.Azure.EnableTracing)
                {
                    provider.AddAzureMonitorTraceExporter(o =>
                    {
                        o.ConnectionString = loggerOptions.Azure.ConnectionString;
                    });
                }

                var jaegerOptions = builder.GetOptions<JaegerOptions>(JaegerOptions.Position);

                if (jaegerOptions?.Enabled == true)
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

                /*
                    Action<ResourceBuilder> appResourceBuilder =
                        resource => resource
                            .AddDetector(new ContainerResourceDetector());

                                builder.Services.AddOpenTelemetry()
                                    .ConfigureResource(appResourceBuilder)
                                    .WithTracing(tracerBuilder => tracerBuilder
                                        .AddRedisInstrumentation(
                                            cartStore.GetConnection(),
                                            options => options.SetVerboseDatabaseStatements = true)
                                        .AddGrpcClientInstrumentation()
                                        .AddHttpClientInstrumentation()
                                        .AddOtlpExporter())

                */
            }).WithMetrics(x =>
            {
                MeterProviderBuilder provider = x.SetResourceBuilder(ResourceBuilder.CreateDefault());

                provider.AddAspNetCoreInstrumentation();

                provider.AddRuntimeInstrumentation();
                provider.AddHttpClientInstrumentation();
                provider.AddOtlpExporter();

                // Check for Console config
                if (loggerOptions.Console?.Enabled == true && loggerOptions.Console.EnableMetrics)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
                    // Any OTEL supportable exporter can be used here
                    provider.AddConsoleExporter();
                }

                // Check for Azure ApplicationInsights config
                if (loggerOptions.Azure?.Enabled == true)
                {
                    provider.AddAzureMonitorMetricExporter(o =>
                    {
                        o.ConnectionString = loggerOptions.Azure.ConnectionString;
                    });
                }
            });

        return builder;
    }
}
