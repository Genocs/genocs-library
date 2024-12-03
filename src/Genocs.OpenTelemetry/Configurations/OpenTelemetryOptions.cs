namespace Genocs.OpenTelemetry.Configurations;

/// <summary>
/// OpenTelemetry Settings.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "jaeger";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    ///
    public bool Enabled { get; set; }

    public string? OtlpEndpoint { get; set; }
}
