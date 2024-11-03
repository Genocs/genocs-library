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

    /// <summary>
    /// The used OtlpExportProtocol.
    /// IT could be [Grpc|HttpProtobuf].
    /// </summary>
    public string Protocol { get; set; } = "Grpc";

    /// <summary>
    /// The used ExportProcessorType.
    /// It could be [Simple|Batch].
    /// </summary>
    public string ProcessorType { get; set; } = "Batch";

    public int MaxQueueSize { get; set; } = 2048;
    public int ScheduledDelayMilliseconds { get; set; } = 5000;
    public int ExporterTimeoutMilliseconds { get; set; } = 30000;
    public int MaxExportBatchSize { get; set; } = 512;
}