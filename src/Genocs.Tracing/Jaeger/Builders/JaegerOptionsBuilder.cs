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

    public IJaegerOptionsBuilder WithMaxPacketSize(int maxPacketSize)
    {
        _options.MaxPacketSize = maxPacketSize;
        return this;
    }

    public IJaegerOptionsBuilder WithMaxTracesPerSecond(double maxTracesPerSecond)
    {
        _options.MaxTracesPerSecond = maxTracesPerSecond;
        return this;
    }

    public IJaegerOptionsBuilder WithSamplingRate(double samplingRate)
    {
        _options.SamplingRate = samplingRate;
        return this;
    }

    public JaegerOptions Build()
        => _options;
}