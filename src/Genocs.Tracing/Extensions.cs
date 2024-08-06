using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Logging.Configurations;
using Genocs.Tracing.Jaeger.Configurations;
using Jaeger.Samplers;
using Jaeger.Senders.Thrift;
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

        var appOptions = builder.GetOptions<AppOptions>(AppOptions.Position);

        // No OpenTelemetryTracing in case of missing ServiceName
        if (string.IsNullOrWhiteSpace(appOptions.Service))
        {
            return builder;
        }

        var services = builder.Services;

        // Set Custom Open telemetry
        services.AddOpenTelemetry().WithTracing(x =>
        {
            TracerProviderBuilder provider = x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                                        .AddService(appOptions.Service)
                                                        .AddTelemetrySdk()
                                                        .AddEnvironmentVariableDetector())
                                                    .AddSource("*");

            var loggerOptions = builder.GetOptions<LoggerOptions>(LoggerOptions.Position);

            // No OpenTelemetryTracing in case of missing LoggerSettings
            if (loggerOptions != null)
            {
                if (loggerOptions.Mongo != null && loggerOptions.Mongo.Enabled)
                {
                    // you should add MongoDB.Driver.Core.Extensions.OpenTelemetry NuGet package
                    provider.AddMongoDBInstrumentation();
                }

                // Check for Console config
                if (loggerOptions.Console != null && loggerOptions.Console.Enabled)
                {
                    // you should add OpenTelemetry.Exporter.Console NuGet package
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
            }

            var jaegerOptions = builder.GetOptions<JaegerOptions>(JaegerOptions.Position);

            if (jaegerOptions != null && jaegerOptions.Enabled)
            {

                provider.AddJaegerExporter(o =>
                {
                    o.AgentHost = jaegerOptions.UdpHost;
                    o.AgentPort = jaegerOptions.UdpPort;
                    o.MaxPayloadSizeInBytes = jaegerOptions.MaxPacketSize;
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
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
                                    .AddAspNetCoreInstrumentation()
                                    .AddGrpcClientInstrumentation()
                                    .AddHttpClientInstrumentation()
                                    .AddOtlpExporter())
                                .WithMetrics(meterBuilder => meterBuilder
                                    .AddRuntimeInstrumentation()
                                    .AddAspNetCoreInstrumentation()
                                    .AddOtlpExporter());
            */
        });

        return builder;
    }

    private static ISampler GetSampler(JaegerOptions options)
    {
        switch (options.Sampler)
        {
            case "const": return new ConstSampler(true);
            case "rate": return new RateLimitingSampler(options.MaxTracesPerSecond);
            case "probabilistic": return new ProbabilisticSampler(options.SamplingRate);
            default: return new ConstSampler(true);
        }
    }

    private static HttpSender BuildHttpSender(JaegerOptions.HttpSenderSettings? options)
    {
        if (options is null)
        {
            throw new Exception("Missing Jaeger HTTP sender options.");
        }

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new Exception("Missing Jaeger HTTP sender endpoint.");
        }

        var builder = new HttpSender.Builder(options.Endpoint);
        if (options.MaxPacketSize > 0)
        {
            builder = builder.WithMaxPacketSize(options.MaxPacketSize);
        }

        if (!string.IsNullOrWhiteSpace(options.AuthToken))
        {
            builder = builder.WithAuth(options.AuthToken);
        }

        if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
        {
            builder = builder.WithAuth(options.Username, options.Password);
        }

        if (!string.IsNullOrWhiteSpace(options.UserAgent))
        {
            builder = builder.WithUserAgent(options.Username);
        }

        return builder.Build();
    }
}
