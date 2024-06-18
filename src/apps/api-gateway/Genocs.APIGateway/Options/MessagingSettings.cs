namespace Genocs.APIGateway.Options;

internal class MessagingSettings
{
    public bool Enabled { get; set; }
    public IEnumerable<EndpointSettings>? Endpoints { get; set; }

    internal class EndpointSettings
    {
        public string? Method { get; set; }
        public string? Path { get; set; }
        public string? Exchange { get; set; }
        public string? RoutingKey { get; set; }
    }
}