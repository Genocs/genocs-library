using Genocs.Common.Types;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Repositories.Clean;

/// <summary>
/// Implements IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository.</typeparam>
public class MongoDbRepository<TEntity> : MongoDbRepositoryBase<TEntity, ObjectId>, IMongoDbRepository<TEntity>
    where TEntity : class, IIdentifiable<ObjectId>
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