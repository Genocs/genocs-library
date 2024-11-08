using Genocs.Persistence.MongoDb.Domain.Entities;
using Genocs.Persistence.MongoDb.Repositories;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Domain.Repositories;

/// <summary>
/// Implements IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository.</typeparam>
public class MongoDbRepository<TEntity> : MongoDbRepositoryBase<TEntity, ObjectId>, IMongoDbRepository<TEntity>
    where TEntity : IMongoDbEntity
{
    /// <summary>
    /// The standard constructor.
    /// </summary>
    /// <param name="databaseProvider">The database provider.</param>
    public MongoDbRepository(IMongoDatabaseProvider databaseProvider)
        : base(databaseProvider)
    {
    }
}