namespace Genocs.Logging.Settings;


/// <summary>
/// Azure application insights logging settings
/// </summary>
public class AzureOptions
{
    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
}