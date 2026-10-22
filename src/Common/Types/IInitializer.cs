namespace Genocs.Common.Types;

/// <summary>
/// The Genocs Initializer interface definition.
/// </summary>
public interface IInitializer
{
    /// <summary>
    /// Standard initializer.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The Task.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}