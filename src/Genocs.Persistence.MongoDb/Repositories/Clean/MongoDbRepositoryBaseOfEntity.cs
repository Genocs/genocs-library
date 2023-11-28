using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Repositories.Clean;

/// <summary>
/// Implements base class for IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository.</typeparam>
public class MongoDbRepositoryBase<TEntity> : MongoDbRepositoryBase<TEntity, ObjectId>
    where TEntity : class, IMongoDbEntity
{
    /// <summary>
    /// The standard constructor.
    /// </summary>
    /// <param name="databaseProvider">The MongoDB database provider.</param>
    public MongoDbRepositoryBase(IMongoDatabaseProvider databaseProvider)
        : base(databaseProvider)
    {
    }
}