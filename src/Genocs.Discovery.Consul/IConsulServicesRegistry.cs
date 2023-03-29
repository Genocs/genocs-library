using Genocs.Discovery.Consul.Models;

namespace Genocs.Discovery.Consul;

public interface IConsulServicesRegistry
{
    Task<ServiceAgent> GetAsync(string name);
}