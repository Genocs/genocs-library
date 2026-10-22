namespace Genocs.LoadBalancing.Fabio.Configurations;

/// <summary>
/// The Fabio settings.
/// </summary>
public class FabioOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "fabio";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The service url.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string? Service { get; set; }
}