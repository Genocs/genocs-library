namespace Genocs.Metrics.AppMetrics.Options;

/// <summary>
/// The MetricsSettings class.
/// </summary>
public class MetricsSettings
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "Metrics";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// It defines whether the Influx db is enabled or not.
    /// </summary>
    public bool InfluxEnabled { get; set; }

    /// <summary>
    /// It defines whether the Prometheus is enabled or not.
    /// </summary>
    public bool PrometheusEnabled { get; set; }

    /// <summary>
    /// The Prometheus formatter.
    /// Allowed method are: protobuf or (null).
    /// </summary>
    public string? PrometheusFormatter { get; set; }

    /// <summary>
    /// The InfluxDb url.
    /// </summary>
    public string? InfluxUrl { get; set; }

    /// <summary>
    /// The InfluxDb database name.
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// The metrics interval.
    /// </summary>
    public int Interval { get; set; } = 10;

    /// <summary>
    /// List of tags.
    /// </summary>
    public IDictionary<string, string>? Tags { get; set; }
}