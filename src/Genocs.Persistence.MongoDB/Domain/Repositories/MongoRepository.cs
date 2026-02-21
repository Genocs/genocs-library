using Genocs.Persistence.MongoDB.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDB.Domain.Repositories;

/// <summary>
/// Implements IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository.</typeparam>
public class MongoRepository<TEntity> : MongoBaseRepositoryOfType<TEntity, ObjectId>, IMongoRepository<TEntity>
    where TEntity : IMongoEntity
{
    /// <summary>
    /// The standard constructor.
    /// </summary>
    /// <param name="databaseProvider">The database provider.</param>
    public MongoRepository(IMongoDatabaseProvider databaseProvider)
        : base(databaseProvider)
    {
    }
}