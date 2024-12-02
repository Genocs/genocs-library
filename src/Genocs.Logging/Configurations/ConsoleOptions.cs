namespace Genocs.Logging.Configurations;

/// <summary>
/// Console Settings.
/// </summary>
public class ConsoleOptions
{
    /// <summary>
    /// It define whether the console logger and tracing are enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// It define whether the console logger will use structured logging or not.
    /// </summary>
    public bool StructuredConsoleLogging { get; set; }

    /// <summary>
    /// It define whether the console logger and tracing are enabled or not.
    /// </summary>
    public bool EnableTracing { get; set; }

    /// <summary>
    /// It define whether the console logger and metrics are enabled or not.
    /// </summary>
    public bool EnableMetrics { get; set; }
}