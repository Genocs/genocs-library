using MongoDB.Driver;
using System.Threading.Tasks;

namespace Genocs.Persistence.MongoDb.Legacy;

public interface IMongoDbSeeder
{
    Task SeedAsync(IMongoDatabase database);
}