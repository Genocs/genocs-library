using MassTransit;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Genocs.Core.Demo.ServiceBusAzure.Worker;

internal static class TelemetryAndLogging
{
    private static DependencyTrackingTelemetryModule? _module;
    private static TelemetryClient? _telemetryClient;
    private static TelemetryConfiguration? _configuration;

    public static void Initialize(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        _module = new DependencyTrackingTelemetryModule();
        _module.IncludeDiagnosticSourceActivities.Add("MassTransit");

        _configuration = TelemetryConfiguration.CreateDefault();
        _configuration.ConnectionString = connectionString;
        _configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

        _telemetryClient = new TelemetryClient(_configuration);
        _module.Initialize(_configuration);


        var loggerOptions = new ApplicationInsightsLoggerOptions();

        var applicationInsightsLoggerProvider = new ApplicationInsightsLoggerProvider(Options.Create(_configuration),
            Options.Create(loggerOptions));

        ILoggerFactory factory = new LoggerFactory();
        factory.AddProvider(applicationInsightsLoggerProvider);

        LogContext.ConfigureCurrentLogContext(factory);
    }

    public static async Task FlushAndCloseAsync()
    {
        _module?.Dispose();
        _telemetryClient?.Flush();
        await Task.Delay(5000);
        _configuration?.Dispose();
    }
}