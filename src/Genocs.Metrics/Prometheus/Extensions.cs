using Genocs.Core.Builders;
using Genocs.Metrics.Prometheus.Configurations;
using Genocs.Metrics.Prometheus.Internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.SystemMetrics;

namespace Genocs.Metrics.Prometheus;

public static class Extensions
{
    public static IGenocsBuilder AddPrometheus(this IGenocsBuilder builder)
    {
        PrometheusOptions options = builder.GetOptions<PrometheusOptions>(PrometheusOptions.Position);
        builder.Services.AddSingleton(options);
        if (!options.Enabled)
        {
            return builder;
        }

        builder.Services.AddHostedService<PrometheusJob>();
        builder.Services.AddSingleton<PrometheusMiddleware>();
        builder.Services.AddSystemMetrics();

        return builder;
    }

    public static IApplicationBuilder UsePrometheus(this IApplicationBuilder app)
    {
        PrometheusOptions options = app.ApplicationServices.GetRequiredService<PrometheusOptions>();
        if (!options.Enabled)
        {
            return app;
        }

        string endpoint = string.IsNullOrWhiteSpace(options.Endpoint) ? "/metrics" :
            options.Endpoint.StartsWith("/") ? options.Endpoint : $"/{options.Endpoint}";

        return app
            .UseMiddleware<PrometheusMiddleware>()
            .UseHttpMetrics()
            .UseGrpcMetrics()
            .UseMetricServer(endpoint);
    }
}