using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDB.Initializers;

/// <summary>
/// The MongoDbInitializer implementation.
/// </summary>
/// <param name="database">The mongoDb database reference.</param>
/// <param name="seeder">The database seeder. The seeder is useful to setup database with custom constraint at setup stage</param>
/// <param name="options">The mongoDb option instance.</param>
internal sealed class MongoInitializer(IMongoDatabase database, IMongoSeeder seeder, MongoOptions options)
    : IMongoInitializer
{
    private static int _initialized;
    private readonly bool _seed = options.Seed;
    private readonly IMongoDatabase _database = database;
    private readonly IMongoSeeder _seeder = seeder;

    /// <summary>
    /// Initialize the database.
    /// </summary>
    /// <returns>The Task.</returns>
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return Task.CompletedTask;
        }

        return _seed ? _seeder.SeedAsync(_database, cancellationToken) : Task.CompletedTask;
    }
}