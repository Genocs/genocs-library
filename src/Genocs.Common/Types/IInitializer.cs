namespace Genocs.Common.Types;

/// <summary>
/// Initializer interface definition
/// </summary>
public interface IInitializer
{
    /// <summary>
    /// Standard initializer
    /// </summary>
    /// <returns></returns>
    Task InitializeAsync();
}