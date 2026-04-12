namespace Genocs.Telemetry.Configurations;

/// <summary>
/// OTLP export telemetry configuration settings.
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
    /// It could be [Grpc|HttpProtobuf].
    /// </summary>
    public string Protocol { get; set; } = "Grpc";

    /// <summary>
    /// The used ExportProcessorType.
    /// It could be [Simple|Batch].
    /// </summary>
    public string ProcessorType { get; set; } = "Batch";

    /// <summary>
    /// Enables OTLP tracing export.
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Enables OTLP metrics export.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// The maximum queue size for OTLP export.
    /// </summary>
    public int MaxQueueSize { get; set; } = 2048;

    /// <summary>
    /// The scheduled delay in milliseconds for OTLP export.
    /// </summary>
    public int ScheduledDelayMilliseconds { get; set; } = 5000;

    /// <summary>
    /// The exporter timeout in milliseconds for OTLP export.
    /// </summary>
    public int ExporterTimeoutMilliseconds { get; set; } = 30000;

    /// <summary>
    /// The maximum export batch size for OTLP export.
    /// </summary>
    public int MaxExportBatchSize { get; set; } = 512;
}