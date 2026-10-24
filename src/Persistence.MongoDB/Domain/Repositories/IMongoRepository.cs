using Genocs.Persistence.MongoDB.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDB.Domain.Repositories;

/// <summary>
/// The MongoDB repository interface.
/// This interface is used to define the contract for the MongoDB repositories when the entity has an ObjectId as the primary key.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IMongoRepository<TEntity> : IMongoBaseRepository<TEntity, ObjectId>
    where TEntity : IMongoEntity;
