namespace Genocs.HTTP;

/// <summary>
/// Generic CorrelationId Factory interface.
/// </summary>
public interface ICorrelationIdFactory
{
    /// <summary>
    /// Create a correlationId.
    /// </summary>
    /// <returns></returns>
    string Create();
}