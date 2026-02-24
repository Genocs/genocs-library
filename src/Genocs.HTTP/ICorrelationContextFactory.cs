namespace Genocs.Http;

/// <summary>
/// The CorrelationContext Factory interface.
/// </summary>
public interface ICorrelationContextFactory
{
    /// <summary>
    /// Create a correlationId.
    /// </summary>
    /// <returns>The correlationId just created.</returns>
    string Create();
}