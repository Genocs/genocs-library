namespace Genocs.Http;

/// <summary>
/// The CorrelationContext Factory interface.
/// </summary>
public interface ICorrelationContextFactory
{
    /// <summary>
    /// Create a correlationId.
    /// </summary>
    /// <returns>The correlationId just created, or null when no value is available.</returns>
    string? Create();
}