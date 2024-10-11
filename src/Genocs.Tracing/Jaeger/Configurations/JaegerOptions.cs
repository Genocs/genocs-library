namespace Genocs.Tracing.Jaeger.Configurations;

/// <summary>
/// Jaeger Settings.
/// </summary>
public class JaegerOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "jaeger";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public string? ServiceName { get; set; }

    /// <summary>
    /// The Jaeger agent endpoint.
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:4317";
    public int MaxPacketSize { get; set; } = 64967;
    public double MaxTracesPerSecond { get; set; } = 5;
    public double SamplingRate { get; set; } = 0.2;
}