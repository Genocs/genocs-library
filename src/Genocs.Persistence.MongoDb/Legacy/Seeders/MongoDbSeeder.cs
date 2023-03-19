using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Legacy.Seeders;

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
        if (collections.Any())
        {
            return;
        }

        await Task.CompletedTask;
    }
}