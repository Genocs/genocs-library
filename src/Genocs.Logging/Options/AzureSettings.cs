namespace Genocs.Logging.Options;

/// <summary>
/// Azure application insights logging settings.
/// </summary>
public class AzureSettings
{
    /// <summary>
    /// It define whether the Azure application insights logger and tracing are enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The Azure application insights connection string.
    /// </summary>
    public string? ConnectionString { get; set; }
}