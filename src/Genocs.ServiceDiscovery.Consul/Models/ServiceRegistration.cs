using System.Text.Json.Serialization;

namespace Genocs.ServiceDiscovery.Consul.Models;

public class ServiceRegistration
{
    [JsonPropertyName("ID")]
    public required string Id { get; init; }
    public string Name { get; init; }
    public List<string> Tags { get; set; }
    public required string Address { get; init; }
    public int Port { get; set; }
    public IDictionary<string, string> Meta { get; init; }
    public bool EnableTagOverride { get; init; }
    public IEnumerable<ServiceCheck> Checks { get; set; }
    public Weights Weights { get; init; }
    public Connect Connect { get; init; }
}