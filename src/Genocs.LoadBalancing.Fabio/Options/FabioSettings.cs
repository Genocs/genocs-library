namespace Genocs.LoadBalancing.Fabio.Options;

/// <summary>
/// The Fabio settings.
/// </summary>
public class FabioSettings
{
    /// <summary>
    /// The default Fabio section name.
    /// </summary>
    public const string Position = "fabio";

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="FabioSettings"/> is enabled.
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