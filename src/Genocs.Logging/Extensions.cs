using Genocs.Common.Options;
using Genocs.Core.Builders;
using Genocs.Logging.Options;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;

namespace Genocs.Logging;

public static class Extensions
{
    internal static LoggingLevelSwitch LoggingLevelSwitch = new();

    public static IHostBuilder UseLogging(this IHostBuilder hostBuilder,
                                            Action<HostBuilderContext, LoggerConfiguration>? configure = null,
                                            string? loggerSectionName = null,
                                            string? appSectionName = null)
        => hostBuilder
            .ConfigureServices(services => services.AddSingleton<ILoggingService, LoggingService>())
            .UseSerilog((context, loggerConfiguration) =>
            {
                if (string.IsNullOrWhiteSpace(loggerSectionName))
                {
                    loggerSectionName = LoggerSettings.Position;
                }

                if (string.IsNullOrWhiteSpace(appSectionName))
                {
                    appSectionName = AppSettings.Position;
                }

                var loggerOptions = context.Configuration.GetOptions<LoggerSettings>(loggerSectionName);
                var appOptions = context.Configuration.GetOptions<AppSettings>(appSectionName);

                MapOptions(loggerOptions, appOptions, loggerConfiguration, context.HostingEnvironment.EnvironmentName);
                configure?.Invoke(context, loggerConfiguration);
            });


    public static IEndpointConventionBuilder MapLogLevelHandler(this IEndpointRouteBuilder builder,
        string endpointRoute = "~/logging/level")
        => builder.MapPost(endpointRoute, LevelSwitch);

    private static void MapOptions(LoggerSettings loggerOptions, AppSettings appOptions,
        LoggerConfiguration loggerConfiguration, string environmentName)
    {
        LoggingLevelSwitch.MinimumLevel = GetLogEventLevel(loggerOptions.Level);

        loggerConfiguration.Enrich.FromLogContext()
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.WithProperty("Application", appOptions.Service)
            .Enrich.WithProperty("Instance", appOptions.Instance)
            .Enrich.WithProperty("Version", appOptions.Version);

        foreach (var (key, value) in loggerOptions.Tags ?? new Dictionary<string, object>())
        {
            loggerConfiguration.Enrich.WithProperty(key, value);
        }

        foreach (var (key, value) in loggerOptions.MinimumLevelOverrides ?? new Dictionary<string, string>())
        {
            var logLevel = GetLogEventLevel(value);
            loggerConfiguration.MinimumLevel.Override(key, logLevel);
        }

        loggerOptions.ExcludePaths?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty<string>("RequestPath", n => n.EndsWith(p))));

        loggerOptions.ExcludeProperties?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty(p)));

        Configure(loggerConfiguration, loggerOptions);
    }

    private static void Configure(LoggerConfiguration loggerConfiguration, LoggerSettings options)
    {
        var consoleOptions = options.Console ?? new ConsoleSettings();
        var fileOptions = options.File ?? new LocalFileSettings();
        var elkOptions = options.Elk ?? new ElkSettings();
        var seqOptions = options.Seq ?? new SeqSettings();
        var lokiOptions = options.Loki ?? new LokiSettings();
        var azureOptions = options.Azure ?? new AzureSettings();

        // console
        if (consoleOptions.Enabled)
        {
            loggerConfiguration.WriteTo.Console();
        }

        // local file system
        if (fileOptions.Enabled)
        {
            var path = string.IsNullOrWhiteSpace(fileOptions.Path) ? "logs/logs.txt" : fileOptions.Path;
            if (!Enum.TryParse<RollingInterval>(fileOptions.Interval, true, out var interval))
            {
                interval = RollingInterval.Day;
            }

            loggerConfiguration.WriteTo.File(path, rollingInterval: interval);
        }

        // elastic search
        if (elkOptions.Enabled)
        {
            loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elkOptions.Url))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                IndexFormat = string.IsNullOrWhiteSpace(elkOptions.IndexFormat)
                    ? "logstash-{0:yyyy.MM.dd}"
                    : elkOptions.IndexFormat,
                ModifyConnectionSettings = connectionConfiguration =>
                    elkOptions.BasicAuthEnabled
                        ? connectionConfiguration.BasicAuthentication(elkOptions.Username, elkOptions.Password)
                        : connectionConfiguration
            }).MinimumLevel.ControlledBy(LoggingLevelSwitch);
        }

        // seq
        if (seqOptions.Enabled)
        {
            loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
        }

        // loki
        if (lokiOptions.Enabled)
        {
            if (lokiOptions.LokiUsername is not null && lokiOptions.LokiPassword is not null)
            {
                var auth = new LokiCredentials
                {
                    Login = lokiOptions.LokiUsername,
                    Password = lokiOptions.LokiPassword
                };

                loggerConfiguration.WriteTo.GrafanaLoki(
                    lokiOptions.Url!,
                    credentials: auth,
                    batchPostingLimit: lokiOptions.BatchPostingLimit ?? 1000,
                    queueLimit: lokiOptions.QueueLimit,
                    period: lokiOptions.Period).MinimumLevel.ControlledBy(LoggingLevelSwitch);
            }
            else
            {
                loggerConfiguration.WriteTo.GrafanaLoki(
                    lokiOptions.Url!,
                    batchPostingLimit: lokiOptions.BatchPostingLimit ?? 1000,
                    queueLimit: lokiOptions.QueueLimit,
                    period: lokiOptions.Period).MinimumLevel.ControlledBy(LoggingLevelSwitch);
            }
        }

        // azure application insights
        if (azureOptions.Enabled)
        {
            loggerConfiguration.WriteTo.ApplicationInsights(new TelemetryConfiguration
            {
                ConnectionString = azureOptions.ConnectionString,
            }, TelemetryConverter.Traces);
        }
    }

    internal static LogEventLevel GetLogEventLevel(string? level)
        => Enum.TryParse<LogEventLevel>(level, true, out var logLevel)
            ? logLevel
            : LogEventLevel.Information;

    public static IGenocsBuilder AddCorrelationContextLogging(this IGenocsBuilder builder)
    {
        builder.Services.AddTransient<CorrelationContextLoggingMiddleware>();

        return builder;
    }

    public static IApplicationBuilder UserCorrelationContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationContextLoggingMiddleware>();

        return app;
    }

    private static async Task LevelSwitch(HttpContext context)
    {
        var service = context.RequestServices.GetService<ILoggingService>();
        if (service is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("ILoggingService is not registered. Add UseLogging() to your Program.cs.");
            return;
        }

        var level = context.Request.Query["level"].ToString();

        if (string.IsNullOrEmpty(level))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid value for logging level.");
            return;
        }

        service.SetLoggingLevel(level);

        context.Response.StatusCode = StatusCodes.Status200OK;
    }
}