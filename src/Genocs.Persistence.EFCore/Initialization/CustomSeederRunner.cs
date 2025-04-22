using Genocs.Common.Persistence.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Persistence.EFCore.Initialization;

/// <summary>
/// CustomSeederRunner is responsible for running custom seeders.
/// </summary>
/// <param name="serviceProvider"></param>
internal class CustomSeederRunner(IServiceProvider serviceProvider)
{
    /// <summary>
    /// Runs the custom seeders.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        ICustomSeeder[] seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();
        foreach (var seeder in seeders)
        {
            await seeder.InitializeAsync(cancellationToken);
        }
    }
}
