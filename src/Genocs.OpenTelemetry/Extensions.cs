using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.OpenTelemetry.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using static Genocs.Core.Exceptions.GenocsException;

namespace Genocs.OpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static IGenocsBuilder AddOpenTelemetry(
                                            this IGenocsBuilder builder)
    {
        AppOptions appOptions = builder.GetOptions<AppOptions>(AppOptions.Position)
            ?? throw new InvalidConfigurationException("app config section is missing. AddOpenTelemetry requires those configuration.");

        // No OpenTelemetryTracing in case of missing ServiceName
        if (string.IsNullOrWhiteSpace(appOptions.Service))
        {
            return builder;
        }

        OpenTelemetryOptions? openTelemetryOptions = builder.GetOptions<OpenTelemetryOptions>(OpenTelemetryOptions.Position);

        //LoggerOptions loggerOptions = builder.GetOptions<LoggerOptions>(LoggerOptions.Position);

        //if (loggerOptions is null)
        //{
        //    return builder;
        //}

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: appOptions.Service))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                metrics
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(openTelemetryOptions.OtlpEndpoint!);
                    });
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                tracing
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(openTelemetryOptions.OtlpEndpoint!);
                    });
            });

        builder.WebApplicationBuilder?.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;

            logging.AddOtlpExporter();
        });

        return builder;
    }

}
