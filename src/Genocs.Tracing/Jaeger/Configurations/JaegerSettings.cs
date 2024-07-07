namespace Genocs.Tracing.Jaeger.Configurations;

/// <summary>
/// Jaeger Settings.
/// </summary>
public class JaegerSettings
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "jaeger";

    /// <summary>
    /// 
    /// </summary>
    public bool Enabled { get; set; }
    public string? ServiceName { get; set; }
    public string? UdpHost { get; set; }
    public int UdpPort { get; set; }
    public int MaxPacketSize { get; set; } = 64967;
    public string? Sampler { get; set; }
    public double MaxTracesPerSecond { get; set; } = 5;
    public double SamplingRate { get; set; } = 0.2;
    public IEnumerable<string>? ExcludePaths { get; set; }
    public string? Sender { get; set; }
    public HttpSenderSettings? HttpSender { get; set; }

    public class HttpSenderSettings
    {
        public string? Endpoint { get; set; }
        public string? AuthToken { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? UserAgent { get; set; }
        public int MaxPacketSize { get; set; } = 1048576;
    }
}