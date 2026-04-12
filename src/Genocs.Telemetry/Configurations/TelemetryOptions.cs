namespace Genocs.Telemetry.Configurations;

/// <summary>
/// OpenTelemetry Settings.
/// </summary>
public class TelemetryOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "telemetry";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    ///
    public bool Enabled { get; set; }

    /// <summary>
    /// The OTLP export telemetry settings.
    /// </summary>
    public OtlpExportOptions? Exporter { get; set; }

    /// <summary>
    /// Console OpenTelemetry settings.
    /// </summary>
    public ConsoleOptions? Console { get; set; }

    /// <summary>
    /// Azure OpenTelemetry settings.
    /// </summary>
    public AzureOptions? Azure { get; set; }

    /// <summary>
    /// MongoDB OpenTelemetry settings.
    /// </summary>
    public MongoDbOptions? MongoDB { get; set; }

    /// <summary>
    /// SQL client OpenTelemetry settings.
    /// </summary>
    public SqlClientOptions? SqlClient { get; set; }

    /// <summary>
    /// Enables wildcard activity source collection (`*`).
    /// Keep disabled unless broad source capture is explicitly required.
    /// </summary>
    public bool EnableWildcardActivitySources { get; set; }

    /// <summary>
    /// Additional activity source names to register for tracing.
    /// This enables future source onboarding without code changes.
    /// </summary>
    public ICollection<string>? ActivitySources { get; set; }
}
