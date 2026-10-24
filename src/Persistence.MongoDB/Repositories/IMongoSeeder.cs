using MongoDB.Driver;

namespace Genocs.Persistence.MongoDB.Repositories;

/// <summary>
/// The MongoDB seeder.
/// </summary>
public interface IMongoSeeder
{
    /// <summary>
    /// Database Seed.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The async Task.</returns>
    Task SeedAsync(IMongoDatabase database, CancellationToken cancellationToken = default);
}