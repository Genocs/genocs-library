using Genocs.Persistence.MongoDb.Domain.Entities;
using Genocs.Persistence.MongoDb.Domain.Repositories;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Repositories;

/// <summary>
/// The MongoDb repository interface.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IMongoDbRepository<TEntity> : IMongoDbBaseRepository<TEntity, ObjectId>
    where TEntity : IMongoDbEntity
{

}
