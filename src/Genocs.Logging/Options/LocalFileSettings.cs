namespace Genocs.Logging.Options;

/// <summary>
/// File settings for local file logging.
/// </summary>
public class LocalFileSettings
{
    /// <summary>
    /// If enabled, local file logging is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The path to the local file.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// The interval to roll the file. it uses the same values as Serilog.Sinks.File.
    /// </summary>
    public string? Interval { get; set; }
}