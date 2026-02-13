using Genocs.Persistence.MongoDb.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Domain.Repositories;

/// <summary>
/// The MongoDb repository interface.
/// This interface is used to define the contract for the MongoDb repositories when the entity has an ObjectId as the primary key.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IMongoDbRepository<TEntity> : IMongoDbBaseRepository<TEntity, ObjectId>
    where TEntity : IMongoDbEntity;
