using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.AspNetCore.Health.Endpoints;
using App.Metrics.AspNetCore.Tracking;
using App.Metrics.Formatters.Prometheus;
using Genocs.Common.Options;
using Genocs.Core.Builders;
using Genocs.Metrics.AppMetrics;
using Genocs.Metrics.AppMetrics.Builders;
using Genocs.Metrics.AppMetrics.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Genocs.Metrics.AppMetrics;

public static class Extensions
{
    private const string MetricsSectionName = "metrics";
    private const string AppSectionName = "app";
    private const string RegistryName = "metrics.metrics";
    private static bool _initialized;

    [Description("For the time being it sets Kestrel's AllowSynchronousIO = true, see https://github.com/AppMetrics/AppMetrics/issues/396")]
    public static IGenocsBuilder AddMetrics(
                                            this IGenocsBuilder builder,
                                            string metricsSectionName = MetricsSectionName,
                                            string appSectionName = AppSectionName)
    {
        if (string.IsNullOrWhiteSpace(metricsSectionName))
        {
            metricsSectionName = MetricsSectionName;
        }

        if (string.IsNullOrWhiteSpace(appSectionName))
        {
            appSectionName = AppSettings.Position;
        }

        var metricsOptions = builder.GetOptions<MetricsSettings>(metricsSectionName);
        var appOptions = builder.GetOptions<AppSettings>(appSectionName);

        return builder.AddMetrics(metricsOptions, appOptions);
    }

    [Description("For the time being it sets Kestrel's AllowSynchronousIO = true, see https://github.com/AppMetrics/AppMetrics/issues/396")]
    public static IGenocsBuilder AddMetrics(
                                            this IGenocsBuilder builder,
                                            Func<IMetricsOptionsBuilder, IMetricsOptionsBuilder> buildOptions,
                                            string appSectionName = AppSectionName)
    {
        if (string.IsNullOrWhiteSpace(appSectionName))
        {
            appSectionName = AppSectionName;
        }

        var metricsOptions = buildOptions(new MetricsOptionsBuilder()).Build();
        var appOptions = builder.GetOptions<AppSettings>(appSectionName);

        return builder.AddMetrics(metricsOptions, appOptions);
    }

    [Description("For the time being it sets Kestrel's and IIS ServerOptions AllowSynchronousIO = true, see https://github.com/AppMetrics/AppMetrics/issues/396")]
    public static IGenocsBuilder AddMetrics(
                                            this IGenocsBuilder builder,
                                            MetricsSettings metricsOptions,
                                            AppSettings appOptions)
    {
        builder.Services.AddSingleton(metricsOptions);
        if (!builder.TryRegister(RegistryName) || !metricsOptions.Enabled || _initialized)
        {
            return builder;
        }

        _initialized = true;

        var metricsBuilder = new MetricsBuilder().Configuration.Configure(cfg =>
        {
            var tags = metricsOptions.Tags;
            if (tags is null)
            {
                return;
            }

            tags.TryGetValue("app", out string? app);
            tags.TryGetValue("env", out string? env);
            tags.TryGetValue("server", out string? server);
            cfg.AddAppTag(string.IsNullOrWhiteSpace(app) ? appOptions.Service : app);
            cfg.AddEnvTag(string.IsNullOrWhiteSpace(env) ? null : env);
            cfg.AddServerTag(string.IsNullOrWhiteSpace(server) ? null : server);
            if (!string.IsNullOrWhiteSpace(appOptions.Instance))
            {
                cfg.GlobalTags.Add("instance", appOptions.Instance);
            }

            if (!string.IsNullOrWhiteSpace(appOptions.Version))
            {
                cfg.GlobalTags.Add("version", appOptions.Version);
            }

            foreach (var tag in tags)
            {
                if (cfg.GlobalTags.ContainsKey(tag.Key))
                {
                    cfg.GlobalTags.Remove(tag.Key);
                }

                if (!cfg.GlobalTags.ContainsKey(tag.Key))
                {
                    cfg.GlobalTags.TryAdd(tag.Key, tag.Value);
                }
            }
        });

        if (metricsOptions.InfluxEnabled)
        {
            metricsBuilder.Report.ToInfluxDb(o =>
            {
                o.InfluxDb.Database = metricsOptions.Database;
                o.InfluxDb.BaseUri = new Uri(metricsOptions.InfluxUrl!);
                o.InfluxDb.CreateDataBaseIfNotExists = true;
                o.FlushInterval = TimeSpan.FromSeconds(metricsOptions.Interval);
            });
        }

        var metrics = metricsBuilder.Build();
        var metricsWebHostOptions = GetMetricsWebHostOptions(metricsOptions);

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var configuration = builder.Configuration ?? serviceProvider.GetRequiredService<IConfiguration>();
        builder.Services.AddHealth();
        builder.Services.AddHealthEndpoints(configuration);
        builder.Services.AddMetricsTrackingMiddleware(configuration);
        builder.Services.AddMetricsEndpoints(configuration);
        builder.Services.AddSingleton<IStartupFilter>(new DefaultMetricsEndpointsStartupFilter());
        builder.Services.AddSingleton<IStartupFilter>(new DefaultHealthEndpointsStartupFilter());
        builder.Services.AddSingleton<IStartupFilter>(new DefaultMetricsTrackingStartupFilter());
        builder.Services.AddMetricsReportingHostedService(metricsWebHostOptions.UnobservedTaskExceptionHandler);
        builder.Services.AddMetricsEndpoints(metricsWebHostOptions.EndpointOptions, configuration);
        builder.Services.AddMetricsTrackingMiddleware(metricsWebHostOptions.TrackingMiddlewareOptions, configuration);
        builder.Services.AddMetrics(metrics);

        return builder;
    }

    public static IApplicationBuilder UseMetrics(this IApplicationBuilder app)
    {
        MetricsOptions options;
        using (var scope = app.ApplicationServices.CreateScope())
        {
            options = scope.ServiceProvider.GetRequiredService<MetricsOptions>();
        }

        return !options.Enabled
            ? app
            : app.UseHealthAllEndpoints()
                .UseMetricsAllEndpoints()
                .UseMetricsAllMiddleware();
    }

    private static MetricsWebHostOptions GetMetricsWebHostOptions(MetricsSettings metricsOptions)
    {
        var options = new MetricsWebHostOptions();

        if (!metricsOptions.Enabled)
        {
            return options;
        }

        if (!metricsOptions.PrometheusEnabled)
        {
            return options;
        }

        options.EndpointOptions = endpointOptions =>
        {
            switch (metricsOptions.PrometheusFormatter?.ToLowerInvariant() ?? string.Empty)
            {
                case "protobuf":
                    endpointOptions.MetricsEndpointOutputFormatter =
                        new MetricsPrometheusProtobufOutputFormatter();
                    break;
                default:
                    endpointOptions.MetricsEndpointOutputFormatter =
                        new MetricsPrometheusTextOutputFormatter();
                    break;
            }
        };

        return options;
    }
}