namespace Genocs.APIGateway.Configurations;

internal class MessagingOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "messaging";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
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