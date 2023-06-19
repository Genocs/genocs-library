namespace Genocs.Core.Demo.WebApi.Options;

public class RabbitMQSettings
{
    public const string Position = "RabbitMQ";

    public string HostName { get; set; } = default!;
    public string VirtualHost { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public int Port { get; set; } = default!;
    public bool UseSSL { get; set; }

}
