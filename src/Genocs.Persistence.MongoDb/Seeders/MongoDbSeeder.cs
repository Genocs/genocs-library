using Genocs.Core.Collections.Extensions;
using Genocs.Persistence.MongoDb.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Seeders;

internal class MongoDbSeeder : IMongoDbSeeder
{
    public async Task SeedAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        await CustomSeedAsync(database, cancellationToken);
    }

    protected virtual async Task CustomSeedAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var cursor = await database.ListCollectionsAsync(cancellationToken: cancellationToken);
        var collections = await cursor.ToListAsync(cancellationToken);
        if (!collections.IsNullOrEmpty())
        {
            return;
        }
    }
}