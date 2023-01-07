using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Genocs.Monitoring
{


    /// <summary>
    /// The Open Telemetry and Tracing
    /// </summary>
    public static class OpenTelemetryInitializer
    {
        /// <summary>
        /// Custom settings for OpenTelemetry
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            string? serviceName = configuration.GetSection("AppSettings")?.GetValue(typeof(string), "ServiceName") as string;

            // No OpenTelemetryTracing in case of missing ServiceName
            if (string.IsNullOrWhiteSpace(serviceName)) return services;

            services.AddOpenTelemetryTracing(x =>
            {
                var providerBuilder = x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                            .AddService(serviceName)
                                            .AddTelemetrySdk()
                                            .AddEnvironmentVariableDetector())
                                        .AddSource("MassTransit")
                                        .AddAspNetCoreInstrumentation()
                                        .AddMongoDBInstrumentation();

                // Check: In case of ApplicationInsights setup Azure Application Insights
                string? connectionString = configuration.GetConnectionString("ApplicationInsights");

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    providerBuilder.AddAzureMonitorTraceExporter(o =>
                    {
                        o.ConnectionString = connectionString;
                    });
                }

                // Check: In case of Monitoring->Jaeger setup Enable Jaeger
                string? jaegerHost = configuration.GetSection("Monitoring")?.GetValue(typeof(string), "Jaeger") as string;
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

            return services;
        }
    }
}
