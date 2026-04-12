namespace Genocs.Telemetry.Configurations;

/// <summary>
/// OTLP export telemetry configuration settings.
/// </summary>
public class OtlpExportOptions
{
    /// <summary>
    /// Enables OTLP exporter registration.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// OTLP exporter endpoint.
    /// Must be an absolute HTTP/HTTPS URI when OTLP export is enabled.
    /// </summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>
    /// OTLP export protocol.
    /// Supported values: <c>Grpc</c> or <c>HttpProtobuf</c>.
    /// </summary>
    public string Protocol { get; set; } = "Grpc";

    /// <summary>
    /// Export processor type.
    /// Supported values: <c>Simple</c> or <c>Batch</c>.
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
    /// Supported range: 512 to 65536. Values outside the range fall back to 2048.
    /// </summary>
    public int MaxQueueSize { get; set; } = 2048;

    /// <summary>
    /// The scheduled delay in milliseconds for OTLP export.
    /// Supported range: 100 to 60000. Values outside the range fall back to 5000.
    /// </summary>
    public int ScheduledDelayMilliseconds { get; set; } = 5000;

    /// <summary>
    /// The exporter timeout in milliseconds for OTLP export.
    /// Supported range: 1000 to 120000. Values outside the range fall back to 30000.
    /// </summary>
    public int ExporterTimeoutMilliseconds { get; set; } = 30000;

    /// <summary>
    /// The maximum export batch size for OTLP export.
    /// Supported range: 1 to 1024. Values outside the range fall back to 512.
    /// The effective value must not exceed MaxQueueSize.
    /// </summary>
    public int MaxExportBatchSize { get; set; } = 512;
}
