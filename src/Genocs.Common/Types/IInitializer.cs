namespace Genocs.Common.Types;

/// <summary>
/// Initializer interface definition.
/// </summary>
public interface IInitializer
{
    /// <summary>
    /// Standard initializer.
    /// </summary>
    /// <returns>The Task.</returns>
    Task InitializeAsync();
}