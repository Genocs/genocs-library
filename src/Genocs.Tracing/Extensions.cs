using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Common.Options;
using Genocs.Core.Builders;
using Genocs.Logging.Options;
using Genocs.Tracing.Jaeger.Options;
using Jaeger.Samplers;
using Jaeger.Senders.Thrift;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Genocs.Tracing;

/// <summary>
/// The Open Telemetry extensions
/// </summary>
public static class Extensions
{

    /// <summary>
    /// Custom settings for OpenTelemetry
    /// </summary>
    /// <param name="builder">The genocs builder</param>
    /// <returns>the builder</returns>
    public static IGenocsBuilder AddOpenTelemetry(this IGenocsBuilder builder)
    {

        var appOptions = builder.GetOptions<AppSettings>(AppSettings.Position);

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

            // TODO> add flag to enable feature MongoDB.Driver.Core.Extensions.OpenTelemetry
            provider.AddMongoDBInstrumentation();


            var loggerOptions = builder.GetOptions<LoggerSettings>(LoggerSettings.Position);


            // Check for Console config
            if (loggerOptions.Console != null && loggerOptions.Console.Enabled)
            {
                // OpenTelemetry.Exporter.Console NuGet package
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

            var jaegerOptions = builder.GetOptions<JaegerSettings>(JaegerSettings.Position);

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
        });

        return builder;
    }

    private static ISampler GetSampler(JaegerSettings options)
    {
        switch (options.Sampler)
        {
            case "const": return new ConstSampler(true);
            case "rate": return new RateLimitingSampler(options.MaxTracesPerSecond);
            case "probabilistic": return new ProbabilisticSampler(options.SamplingRate);
            default: return new ConstSampler(true);
        }
    }

    private static HttpSender BuildHttpSender(JaegerSettings.HttpSenderSettings? options)
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
