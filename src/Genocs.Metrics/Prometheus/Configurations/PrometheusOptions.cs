namespace Genocs.Metrics.Prometheus.Configurations;

/// <summary>
/// The Prometheus Setting definition.
/// </summary>
public class PrometheusOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "prometheus";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; internal set; }

    /// <summary>
    /// The Prometheus endpoint.
    /// </summary>
    public string? Endpoint { get; internal set; }

    /// <summary>
    /// The Prometheus ApiKey.
    /// </summary>
    public string? ApiKey { get; internal set; }

    /// <summary>
    /// The Prometheus AllowedHosts.
    /// </summary>
    public IEnumerable<string>? AllowedHosts { get; internal set; }
}