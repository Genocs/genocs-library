using System.Linq.Expressions;
using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Repositories;
using Genocs.Core.Domain.Entities;

namespace Genocs.Core.Domain.Repositories;

/// <summary>
/// Base class to implement <see cref="IRepository{TEntity, TKey}"/>.
/// It implements some methods in most simple way.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository.</typeparam>
/// <typeparam name="TKey">Type of the Primary Key for this repository.</typeparam>
public abstract class RepositoryBase<TEntity, TKey> : IQueryableRepository<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    public abstract IQueryable<TEntity> GetAll();

    public virtual IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        return GetAll();
    }

    public virtual List<TEntity> GetAllList()
    {
        return [.. GetAll()];
    }

    public virtual Task<List<TEntity>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        return GetAllListCoreAsync(cancellationToken);
    }

    public virtual List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
    {
        return GetAll().Where(predicate).ToList();
    }

    public virtual Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return GetAllListCoreAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Async-first extension point for repositories that can execute list queries natively asynchronous.
    /// The default implementation preserves legacy behavior by delegating to synchronous query materialization.
    /// </summary>
    protected virtual Task<List<TEntity>> GetAllListCoreAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetAllList());
    }

    /// <summary>
    /// Async-first extension point for predicate-based list queries.
    /// The default implementation preserves legacy behavior by delegating to synchronous query materialization.
    /// </summary>
    protected virtual Task<List<TEntity>> GetAllListCoreAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetAllList(predicate));
    }

    public virtual T Query<T>(Func<IQueryable<TEntity>, T> queryMethod)
    {
        return queryMethod(GetAll());
    }

    public virtual TEntity Get(TKey id)
    {
        var entity = FirstOrDefault(id);
        return entity ?? throw new EntityNotFoundException(typeof(TEntity), id);
    }

    public virtual async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await FirstOrDefaultByIdCoreAsync(id, cancellationToken);
        return entity ?? throw new EntityNotFoundException(typeof(TEntity), id);
    }

    public virtual TEntity Single(Expression<Func<TEntity, bool>> predicate)
    {
        return GetAll().Single(predicate);
    }

    public virtual Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return SingleCoreAsync(predicate, cancellationToken);
    }

    public virtual TEntity? FirstOrDefault(TKey id)
    {
        return GetAll().FirstOrDefault(CreateEqualityExpressionForId(id));
    }

    public virtual Task<TEntity?> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultByIdCoreAsync(id, cancellationToken);
    }

    /// <summary>
    /// Async-first extension point for key-based lookup.
    /// The default implementation preserves legacy behavior by delegating to the synchronous path.
    /// </summary>
    protected virtual Task<TEntity?> FirstOrDefaultByIdCoreAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FirstOrDefault(id));
    }

    public virtual TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
    {
        return GetAll().FirstOrDefault(predicate);
    }

    public virtual Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultCoreAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Async-first extension point for predicate-based lookup.
    /// The default implementation preserves legacy behavior by delegating to the synchronous path.
    /// </summary>
    protected virtual Task<TEntity?> FirstOrDefaultCoreAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FirstOrDefault(predicate));
    }

    /// <summary>
    /// Gets an entity with the given primary key asynchronously.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">Primary key of the entity to get.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entity or null if not found.</returns>
    public virtual Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultAsync(id, cancellationToken);
    }

    public virtual TEntity Load(TKey id)
    {
        return Get(id);
    }

    public abstract TEntity Insert(TEntity entity);

    public virtual Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Insert(entity));
    }

    public virtual TKey InsertAndGetId(TEntity entity)
    {
        return Insert(entity).Id;
    }

    public virtual Task<TKey> InsertAndGetIdAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(InsertAndGetId(entity));
    }

    public virtual TEntity InsertOrUpdate(TEntity entity)
    {
        return entity.IsTransient()
            ? Insert(entity)
            : Update(entity);
    }

    public virtual async Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return entity.IsTransient()
            ? await InsertAsync(entity, cancellationToken)
            : await UpdateAsync(entity, cancellationToken);
    }

    public virtual TKey InsertOrUpdateAndGetId(TEntity entity)
    {
        return InsertOrUpdate(entity).Id;
    }

    public virtual Task<TKey> InsertOrUpdateAndGetIdAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(InsertOrUpdateAndGetId(entity));
    }

    public abstract TEntity Update(TEntity entity);

    public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Update(entity));
    }

    public virtual TEntity Update(TKey id, Action<TEntity> updateAction)
    {
        var entity = Get(id);
        updateAction(entity);
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TKey id, Func<TEntity, Task> updateAction, CancellationToken cancellationToken = default)
    {
        var entity = await GetAsync(id, cancellationToken);
        await updateAction(entity);
        return entity;
    }

    public abstract void Delete(TEntity entity);

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Delete(entity);
        return Task.FromResult(0);
    }

    public abstract void Delete(TKey id);

    public virtual Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        Delete(id);
        return Task.FromResult(0);
    }

    public virtual void Delete(Expression<Func<TEntity, bool>> predicate)
    {
        foreach (var entity in GetAll().Where(predicate).ToList())
        {
            Delete(entity);
        }
    }

    public virtual Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        Delete(predicate);
        return Task.FromResult(0);
    }

    public virtual int Count()
    {
        return GetAll().Count();
    }

    public virtual Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return CountCoreAsync(cancellationToken);
    }

    /// <summary>
    /// Async-first extension point for counting all entities.
    /// The default implementation preserves legacy behavior by delegating to the synchronous path.
    /// </summary>
    protected virtual Task<int> CountCoreAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Count());
    }

    public virtual int Count(Expression<Func<TEntity, bool>> predicate)
    {
        return GetAll().Where(predicate).Count();
    }

    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return CountCoreAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Async-first extension point for predicate-based count queries.
    /// The default implementation preserves legacy behavior by delegating to the synchronous path.
    /// </summary>
    protected virtual Task<int> CountCoreAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Count(predicate));
    }

    public virtual long LongCount()
    {
        return GetAll().LongCount();
    }

    public virtual Task<long> LongCountAsync(CancellationToken cancellationToken = default)
    {
        return LongCountCoreAsync(cancellationToken);
    }

    /// <summary>
    /// Async-first extension point for long counting all entities.
    /// The default implementation preserves legacy behavior by delegating to the synchronous path.
    /// </summary>
    protected virtual Task<long> LongCountCoreAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LongCount());
    }

    public virtual long LongCount(Expression<Func<TEntity, bool>> predicate)
    {
        return GetAll().Where(predicate).LongCount();
    }

    public virtual Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return LongCountCoreAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Async-first extension point for predicate-based long count queries.
    /// The default implementation preserves legacy behavior by delegating to the synchronous path.
    /// </summary>
    protected virtual Task<long> LongCountCoreAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LongCount(predicate));
    }

    /// <summary>
    /// Async-first extension point for single-result queries.
    /// The default implementation preserves legacy behavior by delegating to the synchronous path.
    /// </summary>
    protected virtual Task<TEntity> SingleCoreAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Single(predicate));
    }

    protected virtual Expression<Func<TEntity, bool>> CreateEqualityExpressionForId(TKey id)
    {
        var lambdaParam = Expression.Parameter(typeof(TEntity));

        var lambdaBody = Expression.Equal(
            Expression.PropertyOrField(lambdaParam, "Id"),
            Expression.Constant(id, typeof(TKey)));

        return Expression.Lambda<Func<TEntity, bool>>(lambdaBody, lambdaParam);
    }
}
