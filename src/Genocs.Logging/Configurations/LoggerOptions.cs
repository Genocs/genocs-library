using System.Collections.Generic;

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
    public bool Enabled { get; set; } = true;

    public string Level { get; set; }

    /// <summary>
    /// It defines the OpenTelemetry exporter endpoint. In case you are using Serilog with OpenTelemetry.
    /// </summary>
    public string OtlpEndpoint { get; set; }

    /// <summary>
    /// The Console Logging and tracing Settings.
    /// </summary>
    public ConsoleOptions Console { get; set; }
    public LocalFileOptions File { get; set; }
    public ElkOptions Elk { get; set; }
    public SeqOptions Seq { get; set; }

    /// <summary>
    /// Loki logging settings.
    /// </summary>
    public LokiOptions Loki { get; set; }

    /// <summary>
    /// Azure application insights logging settings.
    /// </summary>
    public AzureOptions Azure { get; set; }

    /// <summary>
    /// Optional HTTP payload capture settings.
    /// </summary>
    public HttpPayloadOptions HttpPayload { get; set; }

    public IDictionary<string, string> MinimumLevelOverrides { get; set; }

    /// <summary>
    /// Request paths to exclude from logging. Each entry is matched as a suffix against the
    /// <c>RequestPath</c> property (e.g. <c>/health</c> excludes any path ending with <c>/health</c>).
    /// </summary>
    public IEnumerable<string> ExcludePaths { get; set; }

    /// <summary>
    /// Log property names to exclude from emitted events. Each entry is matched by exact property
    /// name; events containing a property with that name are filtered out entirely.
    /// </summary>
    public IEnumerable<string> ExcludeProperties { get; set; }

    public IDictionary<string, object> Tags { get; set; }
}