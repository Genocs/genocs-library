using Genocs.Persistence.MongoDb.Configurations;
using Genocs.Persistence.MongoDb.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Initializers;

/// <summary>
/// The MongoDbInitializer implementation.
/// </summary>
/// <param name="database">The mongoDb database reference.</param>
/// <param name="seeder">The database seeder. The seeder is useful to setup database with custom constraint at setup stage</param>
/// <param name="options">The mongoDb option instance.</param>
internal sealed class MongoDbInitializer(IMongoDatabase database, IMongoDbSeeder seeder, MongoDbOptions options)
    : IMongoDbInitializer
{
    private static int _initialized;
    private readonly bool _seed = options.Seed;
    private readonly IMongoDatabase _database = database;
    private readonly IMongoDbSeeder _seeder = seeder;

    /// <summary>
    /// Initialize the database.
    /// </summary>
    /// <returns>The Task.</returns>
    public Task InitializeAsync()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return Task.CompletedTask;
        }

        return _seed ? _seeder.SeedAsync(_database) : Task.CompletedTask;
    }
}