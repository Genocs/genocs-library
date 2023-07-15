using Genocs.Persistence.MongoDb.Options;
using Genocs.Persistence.MongoDb.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Initializers;

internal sealed class MongoDbInitializer : IMongoDbInitializer
{
    private static int _initialized;
    private readonly bool _seed;
    private readonly IMongoDatabase _database;
    private readonly IMongoDbSeeder _seeder;

    public MongoDbInitializer(IMongoDatabase database, IMongoDbSeeder seeder, MongoDbSettings options)
    {
        _database = database;
        _seeder = seeder;
        _seed = options.Seed;
    }

    public Task InitializeAsync()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return Task.CompletedTask;
        }

        return _seed ? _seeder.SeedAsync(_database) : Task.CompletedTask;
    }
}