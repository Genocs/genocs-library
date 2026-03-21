using System.Text.Json.Serialization;

namespace Genocs.ServiceDiscovery.Consul.Models;

public class Connect
{
    [JsonPropertyName("sidecar_service")]
    public SidecarService? SidecarService { get; set; }
}