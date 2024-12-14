namespace Genocs.Logging.Configurations;

/// <summary>
/// Elasticsearch Settings.
/// </summary>
public class ElkOptions
{
    /// <summary>
    /// It defines whether the Elasticsearch logger and tracing are enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// It defines whether the Elasticsearch authentication is enabled or not.
    /// </summary>
    public bool BasicAuthEnabled { get; set; }

    /// <summary>
    /// The Elasticsearch Url.
    /// </summary>
    public string? Url { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? IndexFormat { get; set; }
}