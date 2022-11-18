namespace Genocs.Core.Demo.WebApi;

using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

internal static class OpenTelemetryInitializer
{
    public static void Initialize(WebApplicationBuilder builder)
    {

        builder.Services.AddOpenTelemetryTracing(x =>
        {
            string? serviceName = builder.Configuration.GetSection("AppSettings")?.GetValue(typeof(string), "ServiceName") as string;

            var providerBuilder = x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName)
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .AddAspNetCoreInstrumentation();

            string? connectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                providerBuilder.AddAzureMonitorTraceExporter(o =>
                {
                    o.ConnectionString = connectionString;
                });
            }

            string? jaegerHost = builder.Configuration.GetSection("Monitoring")?.GetValue(typeof(string), "Jaeger") as string;
            if (!string.IsNullOrWhiteSpace(jaegerHost))
            {
                providerBuilder.AddJaegerExporter(o =>
                {
                    o.AgentHost = jaegerHost;
                    o.AgentPort = 6831;
                    o.MaxPayloadSizeInBytes = 4096;
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };
                });
            }
        });
    }
}