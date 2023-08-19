using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Common.Options;
using Genocs.Core.Builders;
using Genocs.Logging.Options;
using Genocs.Tracing.Jaeger.Options;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Genocs.Tracing;

/// <summary>
/// The Open Telemetry and Tracing
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

            // Remove comment below to enable tracing on console
            // you should add MongoDB.Driver.Core.Extensions.OpenTelemetry NuGet package
            provider.AddMongoDBInstrumentation();


            var loggerOptions = builder.GetOptions<LoggerSettings>(LoggerSettings.Position);


            // Check for Azure ApplicationInsights 
            if (loggerOptions.Console != null && loggerOptions.Console.Enabled)
            {
                // OpenTelemetry.Exporter.Console NuGet package
                provider.AddConsoleExporter();
            }

            // Check for Azure ApplicationInsights 
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
}
