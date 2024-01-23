namespace Genocs.Metrics.Prometheus.Options;

/// <summary>
/// The Prometheus Setting definition.
/// </summary>
public class PrometheusSettings
{
    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The Prometheus endpoint.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The Prometheus ApiKey.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// The Prometheus AllowedHosts.
    /// </summary>
    public IEnumerable<string>? AllowedHosts { get; set; }
}