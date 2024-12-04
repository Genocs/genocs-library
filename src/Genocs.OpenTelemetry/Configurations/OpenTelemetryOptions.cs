namespace Genocs.GnxOpenTelemetry.Configurations;

/// <summary>
/// OpenTelemetry Settings.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "openTelemetry";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    ///
    public bool Enabled { get; set; }

    public OtlpExportOptions? Exporter { get; set; }

    /// <summary>
    /// Console OpenTelemetry settings.
    /// </summary>
    public ConsoleOptions? Console { get; set; }

    /// <summary>
    /// Azure OpenTelemetry settings.
    /// </summary>
    public AzureOptions? Azure { get; set; }
}
