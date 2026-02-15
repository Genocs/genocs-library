using Genocs.Common.Types;
using Genocs.Core.Collections.Extensions;

namespace Genocs.Core.Builders;

/// <summary>
/// StartupInitializer implementation.
/// </summary>
public class StartupInitializer : IStartupInitializer
{
    private readonly IList<IInitializer> _initializers = [];

    /// <summary>
    /// Add new initializer if not present.
    /// </summary>
    /// <param name="initializer">The initializer to be added.</param>
    public void AddInitializer(IInitializer initializer)
    {
        if (initializer is null)
        {
            return;
        }

        _initializers.AddIfNotContains(initializer);
    }

    /// <summary>
    /// Run the initializer.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The task.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        foreach (var initializer in _initializers)
        {
            await initializer.InitializeAsync(cancellationToken);
        }
    }
}