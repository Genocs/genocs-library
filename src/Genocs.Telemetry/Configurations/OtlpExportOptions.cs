namespace Genocs.Telemetry.Configurations;

/// <summary>
/// OtlpExportOptions Settings.
/// </summary>
public class OtlpExportOptions
{
    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The Otlp Exporter endpoint.
    /// </summary>
    public string? OtlpEndpoint { get; set; }

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