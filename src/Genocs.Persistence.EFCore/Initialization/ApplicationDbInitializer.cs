using Genocs.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genocs.Persistence.EFCore.Initialization;

internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ApplicationDbSeeder _dbSeeder;
    private readonly ILogger<ApplicationDbInitializer> _logger;

    public ApplicationDbInitializer(ApplicationDbContext dbContext, ApplicationDbSeeder dbSeeder, ILogger<ApplicationDbInitializer> logger)
    {

        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSeeder = dbSeeder ?? throw new ArgumentNullException(nameof(dbSeeder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.GetMigrations().Any())
        {
            _logger.LogInformation("Applying migrations to the database.");
        }

        await Task.CompletedTask;
    }
}
