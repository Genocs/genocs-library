using Genocs.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genocs.Persistence.EFCore.Initialization;

/// <summary>
/// ApplicationDbInitializer is responsible for initializing the database.
/// </summary>
internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ApplicationDbSeeder _dbSeeder;
    private readonly ILogger<ApplicationDbInitializer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbInitializer"/> class.
    /// </summary>
    /// <param name="dbContext">The DB Context.</param>
    /// <param name="dbSeeder">the datebase seeder.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">The exception that is thrown when compulsory params are missing.</exception>
    public ApplicationDbInitializer(ApplicationDbContext dbContext, ApplicationDbSeeder dbSeeder, ILogger<ApplicationDbInitializer> logger)
    {

        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSeeder = dbSeeder ?? throw new ArgumentNullException(nameof(dbSeeder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the database.
    /// </summary>
    /// <param name="cancellationToken">The cancelation token.</param>
    /// <returns>The task.</returns>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Skip the initialization if the DB provider is Mongodb
        if (_dbContext.Database.ProviderName == "MongoDB.EntityFrameworkCore")
        {
            return;
        }

        if (_dbContext.Database.GetMigrations().Any())
        {
            _logger.LogInformation("Find migrations that need to be apply. Applying migrations to the database.");
        }

        await Task.CompletedTask;
    }
}
