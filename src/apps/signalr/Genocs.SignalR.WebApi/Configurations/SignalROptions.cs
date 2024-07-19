namespace Genocs.SignalR.WebApi.Configurations;

/// <summary>
/// The signalR settings definition.
/// </summary>
public class SignalROptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "signalR";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public string? Backplane { get; set; }
    public string? Hub { get; set; }
}