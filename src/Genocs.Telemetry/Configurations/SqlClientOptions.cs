namespace Genocs.Telemetry.Configurations;

/// <summary>
/// SQL client telemetry settings.
/// </summary>
public class SqlClientOptions
{
    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Enables SQL command text (`db.query.text` / `db.statement`) on telemetry spans.
    /// Keep disabled by default to reduce the risk of exposing sensitive data.
    /// </summary>
    public bool EnableStatementText { get; set; } = false;
}
