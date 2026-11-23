using System.Linq.Expressions;
using Genocs.Common.CQRS.Queries;
using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDB.Domain.Repositories;

/// <summary>
/// MongoDB repository for an entity with key <typeparamref name="TKey"/>.
/// </summary>
/// <remarks>
/// Documents that implement <see cref="IVersioned"/> use optimistic concurrency.
/// A replacement or a delete of the loaded document succeeds only when the stored <c>Version</c> matches.
/// The repository increments <c>Version</c> when the replacement is accepted and throws
/// <see cref="Genocs.Persistence.MongoDB.Exceptions.OptimisticConcurrencyException"/> when it is not.
/// Delete-by-id and predicate deletes do not check <c>Version</c>.
/// </remarks>
/// <typeparam name="TEntity">Document type.</typeparam>
/// <typeparam name="TKey">Primary key type.</typeparam>
public interface IMongoBaseRepository<TEntity, TKey> : IQueryableRepository<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    IMongoCollection<TEntity> Collection { get; }

    IQueryable<TEntity> GetMongoQueryable();

    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate, TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IPagedQuery;

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}