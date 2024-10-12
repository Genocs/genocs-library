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

namespace Genocs.Tracing;

/// <summary>
/// The Open Telemetry extensions.
/// </summary>
public static class Extensions
{

    /// <summary>
    /// Custom settings for OpenTelemetry.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The Genocs builder you can use for chain.</returns>
    public static IGenocsBuilder AddOpenTelemetry(this IGenocsBuilder builder)
    {

        AppOptions options = builder.GetOptions<AppOptions>(AppOptions.Position);

        // No OpenTelemetryTracing in case of missing ServiceName
        if (string.IsNullOrWhiteSpace(options.Service))
        {
            return builder;
        }

        //builder.Logging.AddOpenTelemetry(logging =>
        //{
        //    logging.IncludeFormattedMessage = true;
        //    logging.IncludeScopes = true;
        //});

        LoggerOptions loggerOptions = builder.GetOptions<LoggerOptions>(LoggerOptions.Position);

        if (loggerOptions is null)
        {
            return builder;
        }

        var services = builder.Services;

        // Set Custom Open telemetry
        services.AddOpenTelemetry()
            .WithTracing(x =>
            {
                TracerProviderBuilder provider = x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                                            .AddService(serviceName: options.Service, serviceVersion: options.Version, serviceInstanceId: options.Instance)
                                                            .AddTelemetrySdk()
                                                            .AddEnvironmentVariableDetector())
                                                            .AddAspNetCoreInstrumentation()
                                                            .AddHttpClientInstrumentation()
                                                        .AddSource("*");

                // No OpenTelemetryTracing in case of missing LoggerSettings
                if (loggerOptions.Mongo != null && loggerOptions.Mongo.Enabled)
                {
                    // you should add MongoDB.Driver.Core.Extensions.OpenTelemetry NuGet package
                    provider.AddMongoDBInstrumentation();
                }

                // Check for Console config
                if (loggerOptions.Console != null && loggerOptions.Console.Enabled)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
                    // Any OTEL supportable exporter can be used here
                    provider.AddConsoleExporter();
                }

                // Check for Azure ApplicationInsights config
                if (loggerOptions.Azure != null && loggerOptions.Azure.Enabled)
                {
                    provider.AddAzureMonitorTraceExporter(o =>
                    {
                        o.ConnectionString = loggerOptions.Azure.ConnectionString;
                    });
                }

                var jaegerOptions = builder.GetOptions<JaegerOptions>(JaegerOptions.Position);

                if (jaegerOptions != null && jaegerOptions.Enabled)
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

                // provider.AddRuntimeInstrumentation();
                provider.AddHttpClientInstrumentation();
                provider.AddOtlpExporter();


                // Check for Console config
                if (loggerOptions.Console != null && loggerOptions.Console.Enabled)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
                    // Any OTEL supportable exporter can be used here
                    provider.AddConsoleExporter();
                }

                // Check for Azure ApplicationInsights config
                if (loggerOptions.Azure != null && loggerOptions.Azure.Enabled)
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
