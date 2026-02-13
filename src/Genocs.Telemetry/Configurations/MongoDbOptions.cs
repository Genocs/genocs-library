namespace Genocs.Telemetry.Configurations;

/// <summary>
/// MongoDB configuration Settings.
/// </summary>
public class MongoDbOptions
{
    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// It defines whether the console tracing are enabled or not.
    /// </summary>
    public bool EnableTracing { get; set; }

    /// <summary>
    /// It defines whether the console metrics are enabled or not.
    /// </summary>
    public bool EnableMetrics { get; set; }

    /// <summary>
    /// It defines whether the console logging are enabled or not.
    /// </summary>
    public bool EnableLogging { get; set; }
}