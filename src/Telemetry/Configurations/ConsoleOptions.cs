namespace Genocs.Telemetry.Configurations;

/// <summary>
/// Console telemetry configuration settings.
/// </summary>
public class ConsoleOptions
{
    /// <summary>
    /// Enables console exporter configuration.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Enables console trace export.
    /// </summary>
    public bool EnableTracing { get; set; }

    /// <summary>
    /// Enables console metrics export.
    /// </summary>
    public bool EnableMetrics { get; set; }
}