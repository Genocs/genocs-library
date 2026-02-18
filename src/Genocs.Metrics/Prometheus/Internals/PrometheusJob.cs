using Genocs.Metrics.Prometheus.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus.DotNetRuntime;

namespace Genocs.Metrics.Prometheus.Internals;

/// <summary>
/// The PrometheusJob that fetch metrics for Prometheus.
/// </summary>
internal sealed class PrometheusJob : IHostedService
{
    private readonly ILogger<PrometheusJob> _logger;
    private readonly bool _enabled;
    private IDisposable? _collector;

    /// <summary>
    /// Default PrometheusJob Constructor.
    /// </summary>
    /// <param name="options">The Prometheus options.</param>
    /// <param name="logger">The logger instance.</param>
    public PrometheusJob(PrometheusOptions options, ILogger<PrometheusJob> logger)
    {
        _enabled = options.Enabled;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation($"Prometheus integration is {(_enabled ? "enabled" : "disabled")}.");
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_enabled)
        {
            _collector = DotNetRuntimeStatsBuilder
                .Customize()
                .WithContentionStats()
                .WithJitStats()
                .WithThreadPoolStats()
                .WithThreadPoolStats()
                .WithGcStats()
                .WithExceptionStats()
                .StartCollecting();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _collector?.Dispose();

        return Task.CompletedTask;
    }
}