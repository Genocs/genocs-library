using Genocs.ServiceDiscovery.Consul.Models;

namespace Genocs.ServiceDiscovery.Consul;

public interface IConsulService
{
    Task<HttpResponseMessage> RegisterServiceAsync(ServiceRegistration registration);
    Task<HttpResponseMessage> DeregisterServiceAsync(string id);
    Task<IDictionary<string, ServiceAgent>?> GetServiceAgentsAsync(string? service = null);
}