namespace Genocs.Logging.Settings;

public class LoggerOptions
{
    public string? Level { get; set; }
    public ConsoleOptions? Console { get; set; }
    public LocalFileOptions? File { get; set; }
    public ElkOptions? Elk { get; set; }
    public SeqOptions? Seq { get; set; }
    public LokiOptions? Loki { get; set; }
    public AzureOptions? Azure { get; set; }

    public IDictionary<string, string>? MinimumLevelOverrides { get; set; }
    public IEnumerable<string>? ExcludePaths { get; set; }
    public IEnumerable<string>? ExcludeProperties { get; set; }
    public IDictionary<string, object>? Tags { get; set; }
}