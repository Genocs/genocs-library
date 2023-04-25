using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Legacy;

public interface IMongoDbSeeder
{
    Task SeedAsync(IMongoDatabase database);
}