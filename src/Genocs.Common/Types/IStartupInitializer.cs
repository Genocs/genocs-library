namespace Genocs.Common.Types;


/// <summary>
/// Startup initializer interface definition
/// </summary>
public interface IStartupInitializer : IInitializer
{
    /// <summary>
    /// It allows to add an initializer
    /// </summary>
    /// <param name="initializer"></param>
    void AddInitializer(IInitializer initializer);
}