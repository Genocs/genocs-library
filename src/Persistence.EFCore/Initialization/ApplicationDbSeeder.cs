using Microsoft.Extensions.Logging;

namespace Genocs.Persistence.EFCore.Initialization;

public class ApplicationDbSeeder
{
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
    {
        _seederRunner = seederRunner ?? throw new ArgumentNullException(nameof(seederRunner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs consumer-provided database seeders (see <see cref="Genocs.Common.Persistence.Initialization.ICustomSeeder"/>).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    public async Task SeedDatabaseAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing custom database seeders.");
        await _seederRunner.RunSeedersAsync(cancellationToken);
        _logger.LogInformation("Custom database seeders finished.");
    }
}