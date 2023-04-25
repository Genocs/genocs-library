namespace Genocs.Logging.Settings;

public class LoggerSettings
{
    public string? Level { get; set; }
    public ConsoleSettings? Console { get; set; }
    public LocalFileSettings? File { get; set; }
    public ElkSettings? Elk { get; set; }
    public SeqSettings? Seq { get; set; }
    public LokiSettings? Loki { get; set; }
    public AzureSettings? Azure { get; set; }

    public IDictionary<string, string>? MinimumLevelOverrides { get; set; }
    public IEnumerable<string>? ExcludePaths { get; set; }
    public IEnumerable<string>? ExcludeProperties { get; set; }
    public IDictionary<string, object>? Tags { get; set; }
}