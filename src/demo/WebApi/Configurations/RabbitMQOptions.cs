namespace Genocs.Library.Demo.WebApi.Configurations;

public class RabbitMQOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "rabbitMQ";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public string HostName { get; set; } = default!;
    public string VirtualHost { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public int Port { get; set; } = default!;
    public bool UseSSL { get; set; }

}
