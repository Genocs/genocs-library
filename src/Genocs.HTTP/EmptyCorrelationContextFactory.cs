namespace Genocs.Http;

internal class EmptyCorrelationContextFactory : ICorrelationContextFactory
{
    public string Create() => default!;
}