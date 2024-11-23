using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.Monitoring.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Genocs.Monitoring;

/// <summary>
/// The Open Telemetry and Tracing.
/// </summary>
public static class OpenTelemetryInitializer
{
    /// <summary>
    /// Custom settings for OpenTelemetry.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        // Read Settings
        string? applicationInsightsConnectionString = configuration.GetConnectionString(Constants.ApplicationInsightsConnectionString);
        string serviceName = configuration.GetValue<string>(Constants.ServiceName) ?? "IntegrationsWorker";

        MonitoringSettings settings = new MonitoringSettings();
        configuration.Bind(MonitoringSettings.Position, settings);
        services.AddSingleton(settings);

        // No OpenTelemetryTracing in case of missing ServiceName
        if (string.IsNullOrWhiteSpace(serviceName)) return services;

        // Set Custom Open telemetry
        services.AddOpenTelemetry().WithTracing(builder =>
        {
            TracerProviderBuilder provider = builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName)
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .AddSource("*");

            // Remove comment below to enable tracing on console
            // you should add MongoDB.Driver.Core.Extensions.OpenTelemetry NuGet package
            provider.AddMongoDBInstrumentation();

            // Remove comment below to enable tracing on console
            // you should add OpenTelemetry.Exporter.Console NuGet package
            provider.AddConsoleExporter();

            // Check for Azure ApplicationInsights.
            if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
            {
                provider.AddAzureMonitorTraceExporter(o =>
                {
                    o.ConnectionString = applicationInsightsConnectionString;
                });
            }

            provider.AddJaegerExporter(o =>
            {
                o.AgentHost = settings.Jaeger;
                o.AgentPort = 6831;
                o.MaxPayloadSizeInBytes = 4096;
                o.ExportProcessorType = ExportProcessorType.Batch;
                o.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                {
                    MaxQueueSize = 2048,
                    ScheduledDelayMilliseconds = 5000,
                    ExporterTimeoutMilliseconds = 30000,
                    MaxExportBatchSize = 512,
                };
            });
        });

        return services;
    }
}
