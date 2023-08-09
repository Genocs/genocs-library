using Genocs.Common.Types;
using Genocs.Core.CQRS.Queries;
using Genocs.Core.Domain.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq.Expressions;

namespace Genocs.Persistence.MongoDb.Repositories;

internal class MongoRepository<TEntity, TIdentifiable> : IMongoRepository<TEntity, TIdentifiable>
    where TEntity : IIdentifiable<TIdentifiable>
{
    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<TEntity>(collectionName);
    }

    public IMongoCollection<TEntity> Collection { get; }

    /// <summary>
    /// It returns the Mongo Collection as Queryable
    /// </summary>
    /// <returns></returns>
    public IMongoQueryable<TEntity> GetMongoQueryable()
    {
        return Collection.AsQueryable();
    }

    public Task<TEntity> GetAsync(TIdentifiable id)
        => GetAsync(e => e.Id.Equals(id));

    public Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
        => Collection.Find(predicate).SingleOrDefaultAsync();

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        => await Collection.Find(predicate).ToListAsync();

    public Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate,
        TQuery query) where TQuery : IPagedQuery
        => Collection.AsQueryable().Where(predicate).PaginateAsync(query);

    /// <summary>
    /// It adds an entity to the Mongo Collection
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public Task AddAsync(TEntity entity)
        => Collection.InsertOneAsync(entity);

    /// <summary>
    /// It updates an entity in the Mongo Collection
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <returns>The updated entity</returns>
    public Task UpdateAsync(TEntity entity)
        => UpdateAsync(entity, e => e.Id.Equals(entity.Id));

    /// <summary>
    /// It updates an entity in the Mongo Collection in async mode
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>The updated entity</returns>
    public Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        => Collection.ReplaceOneAsync(predicate, entity);

    /// <summary>
    /// It deletes an entity from the Mongo Collection in async mode
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task DeleteAsync(TIdentifiable id)
        => DeleteAsync(e => e.Id.Equals(id));

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        => Collection.DeleteOneAsync(predicate);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        => Collection.Find(predicate).AnyAsync();

    public IQueryable<TEntity> GetAll()
        => Collection.AsQueryable();

    public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        throw new NotImplementedException();
    }

    public List<TEntity> GetAllList()
        => Collection.AsQueryable().ToList();

    public async Task<List<TEntity>> GetAllListAsync()
        => await Collection.AsQueryable().ToListAsync();

    public List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
        {
            return GetAllList();
        }

        return Collection.Find(predicate).ToList();
    }

    public async Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
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

    public TEntity Get(TIdentifiable id)
        => Collection.Find(c => c.Id.Equals(id)).First();

    public TEntity Single(Expression<Func<TEntity, bool>> predicate)
        => Collection.Find(predicate).Single();

    public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var result = await Collection.FindAsync(predicate);
        return await result.SingleAsync();
    }

    public TEntity FirstOrDefault(TIdentifiable id)
        => Collection.Find(c => c.Id.Equals(id)).FirstOrDefault();

    public async Task<TEntity> FirstOrDefaultAsync(TIdentifiable id)
    {
        var result = await Collection.FindAsync(c => c.Id.Equals(id));
        return await result.FirstOrDefaultAsync();
    }

    public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        => Collection.Find(predicate).FirstOrDefault();

    public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var result = await Collection.FindAsync(predicate);
        return await result.FirstOrDefaultAsync();
    }

    public TEntity Load(TIdentifiable id)
    {
        throw new NotImplementedException();
    }

    public TEntity Insert(TEntity entity)
    {
        Collection.InsertOne(entity);
        return entity;
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        await Collection.InsertOneAsync(entity);
        return entity;
    }

    public TIdentifiable InsertAndGetId<TIdentifiable>(TEntity entity)
    {
        Collection.InsertOne(entity);
        return entity.Id;
    }

    public Task<TIdentifiable1> InsertAndGetIdAsync<TIdentifiable1>(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public TEntity InsertOrUpdate(TEntity entity)
    {
        Collection.ReplaceOne(c => c.Id.Equals(entity.Id), entity, new ReplaceOptions { IsUpsert = true });
        return entity;
    }

    public async Task<TEntity> InsertOrUpdateAsync(TEntity entity)
    {
        await Collection.ReplaceOneAsync(c => c.Id.Equals(entity.Id), entity, new ReplaceOptions { IsUpsert = true });
        return entity;
    }

    public TIdentifiable InsertOrUpdateAndGetId<TIdentifiable>(TEntity entity)
    {
        return InsertOrUpdate(entity).Id;
    }

    public Task<TIdentifiable1> InsertOrUpdateAndGetIdAsync<TIdentifiable1>(TEntity entity)
    {

        throw new NotImplementedException();
    }

    public TEntity Update(TEntity entity)
    {
        Collection.ReplaceOne(c => c.Id.Equals(entity.Id), entity);
        return entity;
    }


    public TEntity Update(TIdentifiable id, Action<TEntity> updateAction)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> UpdateAsync(TIdentifiable id, Func<TEntity, Task> updateAction)
    {
        throw new NotImplementedException();
    }

    public void Delete(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        Delete(entity.Id);
    }

    public Task DeleteAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        return DeleteAsync(entity.Id);
    }

    public void Delete(TIdentifiable id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }
        Collection.DeleteOne(c => c.Id.Equals(id));
    }

    public void Delete(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }
        Collection.DeleteMany(predicate);
    }

    public int Count()
        => (int)Collection.EstimatedDocumentCount();

    public async Task<int> CountAsync()
        => (int)await Collection.EstimatedDocumentCountAsync();

    public int Count(Expression<Func<TEntity, bool>> predicate)
        => (int)Collection.CountDocuments(predicate);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        => (int)await Collection.EstimatedDocumentCountAsync();

    public long LongCount()
        => Collection.EstimatedDocumentCount();

    public async Task<long> LongCountAsync()
        => await Collection.EstimatedDocumentCountAsync();

    public long LongCount(Expression<Func<TEntity, bool>> predicate)
        => Collection.CountDocuments(predicate);

    public async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate)
        => await Collection.CountDocumentsAsync(predicate);

    async Task<TEntity> IRepositoryOfEntity<TEntity, TIdentifiable>.UpdateAsync(TEntity entity)
    {
        await UpdateAsync(entity, e => e.Id.Equals(entity.Id));
        return entity;
    }
}