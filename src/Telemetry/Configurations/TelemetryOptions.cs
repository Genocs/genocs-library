namespace Genocs.Telemetry.Configurations;

/// <summary>
/// Top-level telemetry configuration for traces and metrics.
/// </summary>
public class TelemetryOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "telemetry";

    /// <summary>
    /// Enables telemetry registration.
    /// When disabled, <c>AddTelemetry()</c> returns without registering providers.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// OTLP exporter settings for traces and metrics.
    /// </summary>
    public OtlpExportOptions? Exporter { get; set; }

    /// <summary>
    /// Console exporter settings for traces and metrics.
    /// </summary>
    public ConsoleOptions? Console { get; set; }

    /// <summary>
    /// Azure Monitor exporter settings for traces and metrics.
    /// </summary>
    public AzureOptions? Azure { get; set; }

    /// <summary>
    /// Prometheus scraping endpoint settings.
    /// When enabled, the OpenTelemetry MeterProvider is augmented with a Prometheus exporter
    /// and the scraping endpoint can be mapped via <c>MapPrometheus()</c>.
    /// </summary>
    public PrometheusOptions? Prometheus { get; set; }

    /// <summary>
    /// MongoDB tracing settings.
    /// MongoDB metrics and log export are not configured by this package.
    /// </summary>
    public MongoDbOptions? MongoDB { get; set; }

    /// <summary>
    /// SQL client OpenTelemetry settings.
    /// </summary>
    public SqlClientOptions? SqlClient { get; set; }

    /// <summary>
    /// Enables wildcard activity source collection (`*`).
    /// Keep disabled unless broad source capture is explicitly required.
    /// </summary>
    public bool EnableWildcardActivitySources { get; set; }

    /// <summary>
    /// Additional activity source names to register for tracing.
    /// This enables future source onboarding without code changes.
    /// </summary>
    public ICollection<string>? ActivitySources { get; set; }

    /// <summary>
    /// Enables fallback to request-path derived values when route metadata is unavailable.
    /// Keep disabled to avoid high-cardinality route tags.
    /// </summary>
    public bool EnableRoutePathFallback { get; set; }

    /// <summary>
    /// Normalizes request-path fallback route values by replacing identifier-like segments.
    /// Only applies when <see cref="EnableRoutePathFallback" /> is enabled.
    /// </summary>
    public bool NormalizeRoutePathFallback { get; set; } = true;
}
