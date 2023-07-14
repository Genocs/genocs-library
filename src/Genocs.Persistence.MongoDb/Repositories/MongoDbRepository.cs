using Genocs.Common.Types;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Repositories;


/// <summary>
/// Implements IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository</typeparam>
public class MongoDbRepository<TEntity> : MongoDbRepositoryBase<TEntity, ObjectId> where TEntity : class, IIdentifiable<ObjectId>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="databaseProvider"></param>
    public MongoDbRepository(IMongoDatabaseProvider databaseProvider)
        : base(databaseProvider)
    {
    }
}