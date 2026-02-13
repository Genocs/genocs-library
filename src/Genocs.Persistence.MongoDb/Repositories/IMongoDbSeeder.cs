using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Repositories;

/// <summary>
/// The MongoDb seeder.
/// </summary>
public interface IMongoDbSeeder
{
    /// <summary>
    /// Database Seed.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The async Task.</returns>
    Task SeedAsync(IMongoDatabase database, CancellationToken cancellationToken = default);
}