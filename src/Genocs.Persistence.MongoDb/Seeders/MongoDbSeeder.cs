using Genocs.Persistence.MongoDb.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Seeders;

internal class MongoDbSeeder : IMongoDbSeeder
{
    public async Task SeedAsync(IMongoDatabase database)
    {
        await CustomSeedAsync(database);
    }

    protected virtual async Task CustomSeedAsync(IMongoDatabase database)
    {
        var cursor = await database.ListCollectionsAsync();
        var collections = await cursor.ToListAsync();
        if (collections.Count != 0)
        {
            return;
        }

        await Task.CompletedTask;
    }
}