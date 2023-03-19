using MongoDB.Driver;
using System.Threading.Tasks;

namespace Genocs.Persistence.MongoDb.Legacy;

public interface IMongoSessionFactory
{
    Task<IClientSessionHandle> CreateAsync();
}