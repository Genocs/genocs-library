namespace Genocs.Discovery.Consul.Configurations;

public class ConsulOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "consul";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; internal set; }

    public string? Url { get; internal set; }
    public string? Service { get; internal set; }
    public string? Address { get; internal set; }
    public int Port { get; internal set; }
    public bool PingEnabled { get; internal set; }
    public string? PingEndpoint { get; internal set; }
    public string? PingInterval { get; internal set; }
    public string? RemoveAfterInterval { get; internal set; }
    public List<string>? Tags { get; internal set; }
    public IDictionary<string, string>? Meta { get; internal set; }
    public bool EnableTagOverride { get; internal set; }
    public bool SkipLocalhostDockerDnsReplace { get; internal set; }
    public ConnectOptions? Connect { get; internal set; }

    public class ConnectOptions
    {
        public bool Enabled { get; internal set; }
    }
}