using Genocs.Core.Builders;
using Genocs.Metrics.Prometheus.Internals;
using Genocs.Metrics.Prometheus.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.SystemMetrics;

namespace Genocs.Metrics.Prometheus;

public static class Extensions
{
    public static IGenocsBuilder AddPrometheus(this IGenocsBuilder builder)
    {
        var prometheusOptions = builder.GetOptions<PrometheusOptions>(PrometheusOptions.Position);
        builder.Services.AddSingleton(prometheusOptions);
        if (!prometheusOptions.Enabled)
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
        var options = app.ApplicationServices.GetRequiredService<PrometheusOptions>();
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