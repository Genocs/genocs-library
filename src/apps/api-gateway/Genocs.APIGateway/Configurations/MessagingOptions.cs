namespace Genocs.APIGateway.Configurations;

internal class MessagingOptions
{
    public const string Position = "messaging";

    public bool Enabled { get; set; }
    public IEnumerable<EndpointOptions>? Endpoints { get; set; }

    internal class EndpointOptions
    {
        public string? Method { get; set; }
        public string? Path { get; set; }
        public string? Exchange { get; set; }
        public string? RoutingKey { get; set; }
    }
}