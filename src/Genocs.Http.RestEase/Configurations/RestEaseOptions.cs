namespace Genocs.Http.RestEase.Configurations;

/// <summary>
/// Represents the options for configuring RestEase HTTP clients.
/// </summary>
public class RestEaseOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "restEase";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public string? LoadBalancer { get; set; }
    public IEnumerable<Service>? Services { get; set; }

    public class Service
    {
        public string Name { get; set; } = default!;
        public string Scheme { get; set; } = default!;
        public string Host { get; set; } = default!;
        public int Port { get; set; }
    }
}