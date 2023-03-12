namespace Genocs.HTTP;

internal class EmptyCorrelationIdFactory : ICorrelationIdFactory
{
    /// <summary>
    /// CorrelationIdFactory, empty implementation 
    /// </summary>
    /// <returns></returns>
    public string? Create() => default;
}