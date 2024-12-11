namespace Genocs.HTTP;

/// <summary>
/// The CorrelationId Factory interface.
/// </summary>
public interface ICorrelationIdFactory
{
    /// <summary>
    /// Create a correlationId.
    /// </summary>
    /// <returns></returns>
    string? Create();
}