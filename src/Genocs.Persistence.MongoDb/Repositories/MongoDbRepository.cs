using Genocs.Common.Types;

namespace Genocs.Persistence.MongoDb.Repositories;


/// <summary>
/// Implements IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository</typeparam>
public class MongoDbRepository<TEntity> : MongoDbRepositoryBase<TEntity, Guid>, IMongoDbRepository<TEntity> where TEntity : class, IIdentifiable<Guid>
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