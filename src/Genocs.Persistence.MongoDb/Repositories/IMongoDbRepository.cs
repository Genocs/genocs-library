using Genocs.Common.Types;
using Genocs.Core.CQRS.Queries;
using Genocs.Core.Domain.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace Genocs.Persistence.MongoDb.Repositories;


/// <summary>
/// The MongoDb repository interface 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TIdentifier"></typeparam>
public interface IMongoDbRepository<TEntity, TIdentifier> : IRepositoryOfEntity<TEntity, TIdentifier> where TEntity : IIdentifiable<TIdentifier>
{
    /// <summary>
    /// Get the Mongo Collection
    /// </summary>
    IMongoCollection<TEntity> Collection { get; }

    /// <summary>
    /// Get the Mongo Collection as Queryable
    /// </summary>
    IMongoQueryable<TEntity> GetMongoQueryable();

    Task<TEntity> GetAsync(TIdentifier id);
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

    Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate, TQuery query) where TQuery : IPagedQuery;

    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate);
    Task DeleteAsync(TIdentifier id);
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
}
