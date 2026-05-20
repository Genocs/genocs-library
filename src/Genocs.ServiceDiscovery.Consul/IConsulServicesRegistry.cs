using Genocs.ServiceDiscovery.Consul.Models;

namespace Genocs.ServiceDiscovery.Consul;

public interface IConsulServicesRegistry
{
    Task<ServiceAgent?> GetAsync(string name);
}