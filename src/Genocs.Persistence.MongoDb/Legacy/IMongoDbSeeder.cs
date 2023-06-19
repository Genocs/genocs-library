using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Legacy;

/// <summary>
/// The MongoDB seeder
/// </summary>
public interface IMongoDbSeeder
{
    /// <summary>
    /// Database Seed 
    /// </summary>
    /// <param name="database"></param>
    /// <returns></returns>
    Task SeedAsync(IMongoDatabase database);
}