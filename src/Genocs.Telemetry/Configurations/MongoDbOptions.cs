namespace Genocs.Telemetry.Configurations;

/// <summary>
/// MongoDB telemetry configuration settings.
/// </summary>
public class MongoDbOptions
{
    /// <summary>
    /// Enables MongoDB telemetry configuration.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Enables MongoDB tracing instrumentation.
    /// MongoDB metrics and log export are not configured by this package.
    /// </summary>
    public bool EnableTracing { get; set; }
}