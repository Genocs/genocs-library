using Genocs.Common.Types;
using Genocs.Persistence.MongoDb.Repositories.Mentor;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Repositories;

/// <summary>
/// The MongoDb repository interface.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IMongoDbRepository<TEntity> : IMongoRepository<TEntity, ObjectId>
    where TEntity : IIdentifiable<ObjectId>
{

}
