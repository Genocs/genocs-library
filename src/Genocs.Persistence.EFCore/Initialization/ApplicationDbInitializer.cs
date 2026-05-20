using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genocs.Persistence.EFCore.Initialization;

/// <summary>
/// ApplicationDbInitializer is responsible for initializing the database.
/// </summary>
internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ApplicationDbSeeder _dbSeeder;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly DatabaseOptions _dbOptions;
    private readonly ILogger<ApplicationDbInitializer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbInitializer"/> class.
    /// </summary>
    public ApplicationDbInitializer(
        ApplicationDbContext dbContext,
        ApplicationDbSeeder dbSeeder,
        IHostApplicationLifetime applicationLifetime,
        IOptions<DatabaseOptions> dbOptions,
        ILogger<ApplicationDbInitializer> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSeeder = dbSeeder ?? throw new ArgumentNullException(nameof(dbSeeder));
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        _dbOptions = dbOptions?.Value ?? throw new ArgumentNullException(nameof(dbOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the database, applying pending migrations when permitted by configuration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // MongoDB EF Core provider does not support migrations.
        if (_dbContext.Database.ProviderName == "MongoDB.EntityFrameworkCore")
        {
            await _dbSeeder.SeedDatabaseAsync(cancellationToken);
            return;
        }

        var pending = (await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pending.Count == 0)
        {
            _logger.LogInformation("No pending migrations found. Database schema is up to date.");
            await _dbSeeder.SeedDatabaseAsync(cancellationToken);
            return;
        }

        if (!_dbOptions.AutoApplyMigrations)
        {
            // Pending migrations exist but automatic application is disabled.
            // This is a deliberate safety gate: migrations must be applied through a
            // controlled pipeline (e.g. a dedicated migration job or CI step).
            _logger.LogError(
                "Database has {Count} pending migration(s): {Migrations}. " +
                "Automatic migration is disabled (DatabaseOptions.AutoApplyMigrations = false). " +
                "Apply migrations manually or set AutoApplyMigrations to true. Stopping the application.",
                pending.Count,
                string.Join(", ", pending));

            _applicationLifetime.StopApplication();
            return;
        }

        _logger.LogInformation(
            "Applying {Count} pending migration(s): {Migrations}.",
            pending.Count,
            string.Join(", ", pending));

        await _dbContext.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Database migrations applied successfully.");

        await _dbSeeder.SeedDatabaseAsync(cancellationToken);
    }
}
