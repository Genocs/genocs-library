namespace Genocs.Common.Configurations;

/// <summary>
/// The application settings.
/// </summary>
public class AppOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "app";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Application name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Service name.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// The instance of the service.
    /// </summary>
    public string? Instance { get; init; }

    /// <summary>
    /// The application version.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// It defines whether the banner is shown into the console at startup time or not.
    /// </summary>
    public bool DisplayBanner { get; init; }

    /// <summary>
    /// It defines whether the application version is shown into the console at startup time or not.
    /// </summary>
    public bool DisplayVersion { get; init; }
}