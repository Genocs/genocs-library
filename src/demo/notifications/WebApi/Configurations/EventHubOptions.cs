namespace Genocs.Notifications.WebApi.Configurations;

public class EventHubOptions
{
    public const string Position = "eventHub";

    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
    public string? Name { get; set; }
}