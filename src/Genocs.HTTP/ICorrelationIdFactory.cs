namespace Genocs.HTTP;

/// <summary>
/// The CorrelationId Factory interface.
/// </summary>
public interface ICorrelationIdFactory
{
    /// <summary>
    /// Create a correlationId.
    /// </summary>
    /// <returns>A string representing the correlation ID, or null if not available.</returns>
    string? Create();
}