namespace Genocs.Logging.Configurations;

/// <summary>
/// Seq Settings.
/// </summary>
public class SeqOptions
{
    /// <summary>
    /// It defines whether the Seq logger and tracing are enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The Seq Url.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The Seq ApiKey.
    /// </summary>
    public string? ApiKey { get; set; }
}