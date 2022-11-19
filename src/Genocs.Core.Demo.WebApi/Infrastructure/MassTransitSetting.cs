namespace Genocs.Core.Demo.WebApi.Infrastructure;

public class MassTransitSettings
{
    public static string Position = "MassTransitSettings";

    public string HostName { get; set; } = default!;
    public string VirtualHost { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}
