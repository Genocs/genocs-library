using Genocs.Common.Types;
using Genocs.Core.Collections.Extensions;

namespace Genocs.Core.Builders;

/// <summary>
/// StartupInitializer implementation.
/// </summary>
public class StartupInitializer(CoreDiagnosticsOptions? diagnosticsOptions = null, CoreDiagnosticsState? diagnosticsState = null) : IStartupInitializer
{
    private readonly IList<IInitializer> _initializers = [];
    private readonly CoreDiagnosticsOptions? _diagnosticsOptions = diagnosticsOptions;
    private readonly CoreDiagnosticsState? _diagnosticsState = diagnosticsState;

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

        if (_diagnosticsOptions?.Enabled == true)
        {
            _diagnosticsState?.AddInfo($"Startup initializer added: '{initializer.GetType().FullName}'.");
        }
    }

    /// <summary>
    /// Run the initializer.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The task.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_diagnosticsOptions?.Enabled == true)
        {
            _diagnosticsState?.AddInfo($"Executing {_initializers.Count} startup initializer(s).");
        }

        foreach (var initializer in _initializers)
        {
            await initializer.InitializeAsync(cancellationToken);
        }
    }
}