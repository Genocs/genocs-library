namespace Genocs.Telemetry.Configurations;

/// <summary>
/// MongoDB telemetry configuration settings.
/// </summary>
public class MongoDbOptions
{
    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// It defines whether the MongoDB tracing are enabled or not.
    /// </summary>
    public bool EnableTracing { get; set; }
}