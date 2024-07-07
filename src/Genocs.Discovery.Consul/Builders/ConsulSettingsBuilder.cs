using Genocs.Discovery.Consul.Configurations;

namespace Genocs.Discovery.Consul.Builders;

internal sealed class ConsulSettingsBuilder : IConsulSettingsBuilder
{
    private readonly ConsulSettings _options = new();

    public IConsulSettingsBuilder Enable(bool enabled)
    {
        _options.Enabled = enabled;
        return this;
    }

    public IConsulSettingsBuilder WithUrl(string url)
    {
        _options.Url = url;
        return this;
    }

    public IConsulSettingsBuilder WithService(string service)
    {
        _options.Service = service;
        return this;
    }

    public IConsulSettingsBuilder WithAddress(string address)
    {
        _options.Address = address;
        return this;
    }

    public IConsulSettingsBuilder WithEnabledPing(bool pingEnabled)
    {
        _options.PingEnabled = pingEnabled;
        return this;
    }

    public IConsulSettingsBuilder WithPingEndpoint(string pingEndpoint)
    {
        _options.PingEndpoint = pingEndpoint;
        return this;
    }

    public IConsulSettingsBuilder WithPingInterval(string pingInterval)
    {
        _options.PingInterval = pingInterval;
        return this;
    }

    public IConsulSettingsBuilder WithRemoteAfterInterval(string remoteAfterInterval)
    {
        _options.RemoveAfterInterval = remoteAfterInterval;
        return this;
    }

    public IConsulSettingsBuilder WithSkippingLocalhostDockerDnsReplace(bool skipLocalhostDockerDnsReplace)
    {
        _options.SkipLocalhostDockerDnsReplace = skipLocalhostDockerDnsReplace;
        return this;
    }

    public ConsulSettings Build() => _options;
}