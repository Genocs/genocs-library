namespace Genocs.HTTP;

internal class EmptyCorrelationIdFactory : ICorrelationIdFactory
{
    public string Create() => default;
}