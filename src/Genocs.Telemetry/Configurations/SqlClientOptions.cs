namespace Genocs.Telemetry.Configurations;

/// <summary>
/// SQL client telemetry settings.
/// </summary>
public class SqlClientOptions
{
    /// <summary>
    /// Enables SQL command text (`db.query.text` / `db.statement`) on telemetry spans.
    /// Keep disabled by default to reduce the risk of exposing sensitive data.
    /// </summary>
    public bool EnableStatementText { get; set; } = false;
}
