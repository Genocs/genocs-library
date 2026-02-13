using App.Metrics;
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Metrics.AppMetrics.Builders;
using Genocs.Metrics.AppMetrics.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Metrics.AppMetrics;

public static class Extensions
{
    private const string RegistryName = "metrics.metrics";
    private static bool _initialized;

    public static IGenocsBuilder AddMetrics(
                                            this IGenocsBuilder builder,
                                            string metricsSectionName = Configurations.MetricsOptions.Position,
                                            string appSectionName = AppOptions.Position)
    {
        if (string.IsNullOrWhiteSpace(metricsSectionName))
        {
            metricsSectionName = Configurations.MetricsOptions.Position;
        }

        if (string.IsNullOrWhiteSpace(appSectionName))
        {
            appSectionName = AppOptions.Position;
        }

        var metricsOptions = builder.GetOptions<Configurations.MetricsOptions>(metricsSectionName);
        var appOptions = builder.GetOptions<AppOptions>(appSectionName);

        return builder.AddMetrics(metricsOptions, appOptions);
    }

    public static IGenocsBuilder AddMetrics(
                                            this IGenocsBuilder builder,
                                            Func<IMetricsOptionsBuilder, IMetricsOptionsBuilder> buildOptions,
                                            string appSectionName = AppOptions.Position)
    {
        if (string.IsNullOrWhiteSpace(appSectionName))
        {
            appSectionName = AppOptions.Position;
        }

        var metricsOptions = buildOptions(new MetricsOptionsBuilder()).Build();
        var appOptions = builder.GetOptions<AppOptions>(appSectionName);

        return builder.AddMetrics(metricsOptions, appOptions);
    }

    public static IGenocsBuilder AddMetrics(
                                            this IGenocsBuilder builder,
                                            Configurations.MetricsOptions metricsSettings,
                                            AppOptions appSettings)
    {
        builder.Services.AddSingleton(metricsSettings);
        if (!builder.TryRegister(RegistryName) || !metricsSettings.Enabled || _initialized)
        {
            return builder;
        }

        _initialized = true;

        var metricsBuilder = new MetricsBuilder().Configuration.Configure(cfg =>
        {
            var tags = metricsSettings.Tags;
            if (tags is null)
            {
                return;
            }

            tags.TryGetValue("app", out string? app);
            tags.TryGetValue("env", out string? env);
            tags.TryGetValue("server", out string? server);
            cfg.AddAppTag(string.IsNullOrWhiteSpace(app) ? appSettings.Service : app);
            cfg.AddEnvTag(string.IsNullOrWhiteSpace(env) ? null : env);
            cfg.AddServerTag(string.IsNullOrWhiteSpace(server) ? null : server);
            if (!string.IsNullOrWhiteSpace(appSettings.Instance))
            {
                cfg.GlobalTags.Add("instance", appSettings.Instance);
            }

            if (!string.IsNullOrWhiteSpace(appSettings.Version))
            {
                cfg.GlobalTags.Add("version", appSettings.Version);
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

        if (metricsSettings.InfluxEnabled)
        {
            metricsBuilder.Report.ToInfluxDb(o =>
            {
                o.InfluxDb.Database = metricsSettings.Database;
                o.InfluxDb.BaseUri = new Uri(metricsSettings.InfluxUrl!);
                o.InfluxDb.CreateDataBaseIfNotExists = true;
                o.FlushInterval = TimeSpan.FromSeconds(metricsSettings.Interval);
            });
        }

        return builder;
    }

    public static IApplicationBuilder UseMetrics(this IApplicationBuilder app)
    {
        if (!_initialized)
        {
            return app;
        }

        App.Metrics.MetricsOptions options;
        using (var scope = app.ApplicationServices.CreateScope())
        {
            options = scope.ServiceProvider.GetRequiredService<App.Metrics.MetricsOptions>();
        }

        return app;
    }
}