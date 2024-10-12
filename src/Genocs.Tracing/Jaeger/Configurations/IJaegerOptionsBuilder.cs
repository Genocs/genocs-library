namespace Genocs.Tracing.Jaeger.Configurations;

public interface IJaegerOptionsBuilder
{
    IJaegerOptionsBuilder Enable(bool enabled);
    IJaegerOptionsBuilder WithServiceName(string serviceName);
    IJaegerOptionsBuilder WithEndpoint(string endpoint);
    IJaegerOptionsBuilder WithProtocol(string protocol);
    IJaegerOptionsBuilder WithProcessorType(string processorType);
    IJaegerOptionsBuilder WithMaxQueueSize(int maxQueueSize);
    IJaegerOptionsBuilder WithScheduledDelayMilliseconds(int scheduledDelayMilliseconds);
    IJaegerOptionsBuilder WithExporterTimeoutMilliseconds(int exporterTimeoutMilliseconds);
    IJaegerOptionsBuilder WithMaxExportBatchSize(int maxExportBatchSize);
    JaegerOptions Build();
}