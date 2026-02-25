using System.Linq.Expressions;
using Genocs.Common.CQRS.Queries;
using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Repositories;
using Genocs.Persistence.MongoDB.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Genocs.Persistence.MongoDB.Domain.Repositories;

internal class MongoBaseRepository<TEntity, TKey> : IMongoBaseRepository<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    public MongoBaseRepository(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<TEntity>(collectionName);
    }

    public IMongoCollection<TEntity> Collection { get; }

    /// <summary>
    /// It returns the Mongo Collection as Queryable.
    /// </summary>
    /// <returns>The Mongo Collection as Queryable.</returns>
    public IQueryable<TEntity> GetMongoQueryable()
    {
        return Collection.AsQueryable();
    }

    /// <summary>
    /// This method retrieves an entity from the Mongo Collection based on the provided ID.
    /// It uses the GetAsync method with a predicate to find the entity where the ID matches the provided ID. If no entity is found, it returns null.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
        => GetAsync(e => e.Id.Equals(id), cancellationToken);

    public Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Collection.Find(predicate).SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await Collection.Find(predicate).ToListAsync(cancellationToken);

    public Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate, TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IPagedQuery
        => Collection.AsQueryable().Where(predicate).PaginateAsync(query);

    /// <summary>
    /// It adds an entity to the Mongo Collection.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

    /// <summary>
    /// It updates an entity in the Mongo Collection.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated entity.</returns>
    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await UpdateAsync(entity, e => e.Id.Equals(entity.Id), cancellationToken);
        return entity;
    }

    /// <summary>
    /// It updates an entity in the Mongo Collection in async mode.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated entity.</returns>
    public Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Collection.ReplaceOneAsync(predicate, entity, cancellationToken: cancellationToken);

    /// <summary>
    /// It deletes an entity from the Mongo Collection in async mode.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        => DeleteAsync(e => e.Id.Equals(id));

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Collection.DeleteOneAsync(predicate);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Collection.Find(predicate).AnyAsync();

    public IQueryable<TEntity> GetAll()
        => Collection.AsQueryable();

    public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors)
        => GetAll();

    public List<TEntity> GetAllList()
        => Collection.AsQueryable().ToList();

    public async Task<List<TEntity>> GetAllListAsync(CancellationToken cancellationToken = default)
        => await Collection.AsQueryable().ToListAsync();

    public List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
        {
            return GetAllList();
        }

        return Collection.Find(predicate).ToList();
    }

    public async Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await GetAllListAsync();
        }

        return await Collection.Find(predicate).ToListAsync();
    }

    public T Query<T>(Func<IQueryable<TEntity>, T> queryMethod)
    {
        throw new NotImplementedException();
    }

    public TEntity Get(TKey id)
        => Collection.Find(c => c.Id.Equals(id)).First();

    public TEntity Single(Expression<Func<TEntity, bool>> predicate)
        => Collection.Find(predicate).Single();

    public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = await Collection.FindAsync(predicate);
        return await result.SingleAsync();
    }

    public TEntity? FirstOrDefault(TKey id)
        => Collection.Find(c => c.Id.Equals(id)).FirstOrDefault();

    public async Task<TEntity?> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var result = await Collection.FindAsync(c => c.Id.Equals(id));
        return await result.FirstOrDefaultAsync();
    }

    public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        => Collection.Find(predicate).FirstOrDefault();

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = await Collection.FindAsync(predicate);
        return await result.FirstOrDefaultAsync();
    }

    public TEntity? Load(TKey id)
        => FirstOrDefault(id);

    public TEntity Insert(TEntity entity)
    {
        Collection.InsertOne(entity);
        return entity;
    }

    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Collection.InsertOneAsync(entity);
        return entity;
    }

    public TKey InsertAndGetId(TEntity entity)
        => Insert(entity).Id;

    public async Task<TKey> InsertAndGetIdAsync(TEntity entity, CancellationToken cancellationToken = default)
        => (await InsertAsync(entity)).Id;

    public TEntity InsertOrUpdate(TEntity entity)
    {
        Collection.ReplaceOne(c => c.Id!.Equals(entity.Id), entity, new ReplaceOptions { IsUpsert = true });
        return entity;
    }

    public async Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Collection.ReplaceOneAsync(c => c.Id!.Equals(entity.Id), entity, new ReplaceOptions { IsUpsert = true });
        return entity;
    }

    public TKey InsertOrUpdateAndGetId(TEntity entity)
        => InsertOrUpdate(entity).Id;

    public async Task<TKey> InsertOrUpdateAndGetIdAsync(TEntity entity, CancellationToken cancellationToken = default)
        => (await InsertOrUpdateAsync(entity)).Id;

    public TEntity Update(TEntity entity)
    {
        Collection.ReplaceOne(c => c.Id!.Equals(entity.Id), entity);
        return entity;
    }

    public TEntity Update(TKey id, Action<TEntity> updateAction)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> UpdateAsync(TKey id, Func<TEntity, Task> updateAction, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Delete(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        Delete(entity.Id);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        return DeleteAsync(entity.Id);
    }

    public void Delete(TKey id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        Collection.DeleteOne(c => c.Id!.Equals(id));
    }

    public void Delete(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        Collection.DeleteMany(predicate);
    }

    public int Count()
        => (int)Collection.EstimatedDocumentCount();

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => (int)await Collection.EstimatedDocumentCountAsync();

    public int Count(Expression<Func<TEntity, bool>> predicate)
        => (int)Collection.CountDocuments(predicate);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => (int)await Collection.EstimatedDocumentCountAsync();

    public long LongCount()
        => Collection.EstimatedDocumentCount();

    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await Collection.EstimatedDocumentCountAsync();

    public long LongCount(Expression<Func<TEntity, bool>> predicate)
        => Collection.CountDocuments(predicate);

    public async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await Collection.CountDocumentsAsync(predicate);

    Task<TEntity> IRepositoryOfEntity<TEntity, TKey>.UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}