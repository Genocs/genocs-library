using Genocs.Core.CQRS.Queries;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace Genocs.Persistence.MongoDb.Repositories;


/// <summary>
/// Implements base class for IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository</typeparam>
public class MongoDbRepositoryBase<TEntity> : MongoDbRepositoryBase<TEntity, Guid>
    where TEntity : class, IMongoDbEntity
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="databaseProvider"></param>
    public MongoDbRepositoryBase(IMongoDatabaseProvider databaseProvider)
        : base(databaseProvider)
    {
    }

    /// <summary>
    /// It returns the Mongo Collection as Queryable
    /// </summary>
    /// <returns></returns>
    public IMongoQueryable<TEntity> GetMongoQueryable()
    {
        return Collection.AsQueryable();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    /// <param name="predicate"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate,
        TQuery query) where TQuery : IPagedQuery
            => await Collection.AsQueryable().Where(predicate).PaginateAsync(query);
}