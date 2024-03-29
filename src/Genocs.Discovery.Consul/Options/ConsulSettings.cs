namespace Genocs.Discovery.Consul.Options;

public class ConsulSettings
{
    public bool Enabled { get; set; }
    public string? Url { get; set; }
    public string? Service { get; set; }
    public string? Address { get; set; }
    public int Port { get; set; }
    public bool PingEnabled { get; set; }
    public string? PingEndpoint { get; set; }
    public string? PingInterval { get; set; }
    public string? RemoveAfterInterval { get; set; }
    public List<string>? Tags { get; set; }
    public IDictionary<string, string>? Meta { get; set; }
    public bool EnableTagOverride { get; set; }
    public bool SkipLocalhostDockerDnsReplace { get; set; }
    public ConnectSettings? Connect { get; set; }

    public class ConnectSettings
    {
        public bool Enabled { get; set; }
    }
}