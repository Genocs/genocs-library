namespace Genocs.Core.Builders;

using Genocs.Core.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// StartupInitializer implementation
/// </summary>
public class StartupInitializer : IStartupInitializer
{
    private readonly IList<IInitializer> _initializers = new List<IInitializer>();


    /// <summary>
    /// Add new initializer if not present
    /// </summary>
    /// <param name="initializer"></param>
    public void AddInitializer(IInitializer initializer)
    {
        if (initializer is null || _initializers.Contains(initializer))
        {
            return;
        }

        _initializers.Add(initializer);
    }

    /// <summary>
    /// Run the initializer
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        foreach (var initializer in _initializers)
        {
            await initializer.InitializeAsync();
        }
    }
}