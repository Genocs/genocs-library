namespace Genocs.HTTP;

/// <summary>
/// Provides an implementation of the ICorrelationIdFactory interface that always returns null, indicating that no
/// correlation ID is available.
/// </summary>
/// <remarks>Use this class in scenarios where correlation IDs are not required or should be omitted. This
/// implementation is suitable for environments where correlation tracking is unnecessary or disabled.</remarks>
internal class EmptyCorrelationIdFactory : ICorrelationIdFactory
{
    /// <summary>
    /// CorrelationIdFactory, empty implementation.
    /// </summary>
    /// <returns>A string representing the correlation ID, or null if not available.</returns>
    public string? Create() => default;
}