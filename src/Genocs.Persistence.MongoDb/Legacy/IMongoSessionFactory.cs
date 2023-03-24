using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Legacy;

public interface IMongoSessionFactory
{
    Task<IClientSessionHandle> CreateAsync();
}