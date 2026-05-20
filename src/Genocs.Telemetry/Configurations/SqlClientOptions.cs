namespace Genocs.Telemetry.Configurations;

/// <summary>
/// SQL client telemetry settings.
/// </summary>
public class SqlClientOptions
{
    /// <summary>
    /// Enables SQL client tracing instrumentation.
    /// Defaults to true to preserve backward compatibility for existing adopters.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Enables SQL command text (`db.query.text` / `db.statement`) on telemetry spans.
    /// Keep disabled by default to reduce the risk of exposing sensitive data.
    /// </summary>
    public bool EnableStatementText { get; set; }
}
