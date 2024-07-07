namespace Genocs.Discovery.Consul.Configurations;

public interface IConsulSettingsBuilder
{
    IConsulSettingsBuilder Enable(bool enabled);
    IConsulSettingsBuilder WithUrl(string url);
    IConsulSettingsBuilder WithService(string service);
    IConsulSettingsBuilder WithAddress(string address);
    IConsulSettingsBuilder WithEnabledPing(bool pingEnabled);
    IConsulSettingsBuilder WithPingEndpoint(string pingEndpoint);
    IConsulSettingsBuilder WithPingInterval(string pingInterval);
    IConsulSettingsBuilder WithRemoteAfterInterval(string remoteAfterInterval);
    IConsulSettingsBuilder WithSkippingLocalhostDockerDnsReplace(bool skipLocalhostDockerDnsReplace);
    ConsulSettings Build();
}