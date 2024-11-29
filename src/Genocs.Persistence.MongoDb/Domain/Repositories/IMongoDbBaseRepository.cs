using System.Linq.Expressions;
using Genocs.Core.CQRS.Queries;
using Genocs.Core.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Domain.Repositories;

public interface IMongoDbBaseRepository<TEntity, TKey> : IRepositoryOfEntity<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    IMongoCollection<TEntity> Collection { get; }

    IQueryable<TEntity> GetMongoQueryable();

    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

    Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate, TQuery query)
        where TQuery : IPagedQuery;

    Task AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
}