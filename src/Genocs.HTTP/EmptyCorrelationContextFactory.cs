namespace Genocs.HTTP;

internal class EmptyCorrelationContextFactory : ICorrelationContextFactory
{
    public string Create() => default;
}