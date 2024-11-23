using Genocs.Tracing.Jaeger.Configurations;

namespace Genocs.Tracing.Jaeger.Builders;

internal sealed class JaegerOptionsBuilder : IJaegerOptionsBuilder
{
    private readonly JaegerOptions _options = new();

    public IJaegerOptionsBuilder Enable(bool enabled)
    {
        _options.Enabled = enabled;
        return this;
    }

    public IJaegerOptionsBuilder WithServiceName(string serviceName)
    {
        _options.ServiceName = serviceName;
        return this;
    }

    public IJaegerOptionsBuilder WithEndpoint(string endpoint)
    {
        _options.Endpoint = endpoint;
        return this;
    }

    public IJaegerOptionsBuilder WithProtocol(string protocol)
    {
        _options.Protocol = protocol;
        return this;
    }

    public IJaegerOptionsBuilder WithProcessorType(string processorType)
    {
        _options.ProcessorType = processorType;
        return this;
    }

    public IJaegerOptionsBuilder WithMaxQueueSize(int maxQueueSize)
    {
        _options.MaxQueueSize = maxQueueSize;
        return this;
    }

    public IJaegerOptionsBuilder MaxQueueSize(int maxQueueSize)
    {
        _options.MaxQueueSize = maxQueueSize;
        return this;
    }

    public IJaegerOptionsBuilder WithScheduledDelayMilliseconds(int scheduledDelayMilliseconds)
    {
        _options.ScheduledDelayMilliseconds = scheduledDelayMilliseconds;
        return this;
    }

    public IJaegerOptionsBuilder WithExporterTimeoutMilliseconds(int exporterTimeoutMilliseconds)
    {
        _options.ExporterTimeoutMilliseconds = exporterTimeoutMilliseconds;
        return this;
    }

    public IJaegerOptionsBuilder WithMaxExportBatchSize(int maxExportBatchSize)
    {
        _options.MaxExportBatchSize = maxExportBatchSize;
        return this;
    }

    public JaegerOptions Build()
        => _options;
}