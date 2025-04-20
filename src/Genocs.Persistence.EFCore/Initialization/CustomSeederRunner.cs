using Genocs.Common.Persistence.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Persistence.EFCore.Initialization;

internal class CustomSeederRunner(IServiceProvider serviceProvider)
{
    private readonly ICustomSeeder[] _seeders = [.. serviceProvider.GetServices<ICustomSeeder>()];

    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.InitializeAsync(cancellationToken);
        }
    }
}