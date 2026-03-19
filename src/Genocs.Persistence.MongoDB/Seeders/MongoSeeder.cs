using Genocs.Core.Collections.Extensions;
using Genocs.Persistence.MongoDB.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDB.Seeders;

internal class MongoSeeder : IMongoSeeder
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