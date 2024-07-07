namespace Genocs.Logging.Configurations;

/// <summary>
/// Azure application insights logging settings.
/// </summary>
public class AzureOptions
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