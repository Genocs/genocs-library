namespace Genocs.Core.Demo.WebApi.Options;

public class RabbitMQSettings
{
    public static string Position = "RabbitMQSettings";

    public string HostName { get; set; } = default!;
    public string VirtualHost { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}
