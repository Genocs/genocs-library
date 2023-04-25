using System.Text.Json.Serialization;

namespace Genocs.Discovery.Consul.Models;

public class Connect
{
    [JsonPropertyName("sidecar_service")]
    public SidecarService SidecarService { get; set; }
}