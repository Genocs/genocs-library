namespace Genocs.Tracing.Jaeger.Configurations;

public interface IJaegerOptionsBuilder
{
    IJaegerOptionsBuilder Enable(bool enabled);
    IJaegerOptionsBuilder WithServiceName(string serviceName);
    IJaegerOptionsBuilder WithEndpoint(string endpoint);
    IJaegerOptionsBuilder WithMaxPacketSize(int maxPacketSize);
    IJaegerOptionsBuilder WithMaxTracesPerSecond(double maxTracesPerSecond);
    IJaegerOptionsBuilder WithSamplingRate(double samplingRate);
    JaegerOptions Build();
}