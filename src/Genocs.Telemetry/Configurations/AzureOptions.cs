namespace Genocs.Telemetry.Configurations;

/// <summary>
/// Azure Monitor exporter settings.
/// </summary>
public class AzureOptions
{
    /// <summary>
    /// Enables Azure Monitor exporter configuration.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Azure Monitor connection string.
    /// A non-empty value is required for exporter registration.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Enables Azure trace export.
    /// </summary>
    public bool EnableTracing { get; set; }

    /// <summary>
    /// Enables Azure metrics export.
    /// </summary>
    public bool EnableMetrics { get; set; }
}