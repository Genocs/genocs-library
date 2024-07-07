namespace Genocs.Logging.Configurations;

/// <summary>
/// Logger Settings.
/// </summary>
public class LoggerOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "logger";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public string? Level { get; set; }

    /// <summary>
    /// The Console Logging and tracing Settings.
    /// </summary>
    public ConsoleOptions? Console { get; set; }
    public LocalFileOptions? File { get; set; }
    public ElkOptions? Elk { get; set; }
    public SeqOptions? Seq { get; set; }

    /// <summary>
    /// Loki logging settings.
    /// </summary>
    public LokiOptions? Loki { get; set; }

    /// <summary>
    /// Azure application insights logging settings.
    /// </summary>
    public AzureOptions? Azure { get; set; }

    /// <summary>
    /// MongoDb logging settings.
    /// </summary>
    public MongoOptions? Mongo { get; set; }

    public IDictionary<string, string>? MinimumLevelOverrides { get; set; }
    public IEnumerable<string>? ExcludePaths { get; set; }
    public IEnumerable<string>? ExcludeProperties { get; set; }
    public IDictionary<string, object>? Tags { get; set; }
}