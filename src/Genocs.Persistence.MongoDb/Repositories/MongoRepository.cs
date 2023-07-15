using Genocs.Common.Types;
using Genocs.Core.CQRS.Queries;
using Genocs.Core.Domain.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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

    public Task AddAsync(TEntity entity)
        => Collection.InsertOneAsync(entity);

    public Task UpdateAsync(TEntity entity)
        => UpdateAsync(entity, e => e.Id.Equals(entity.Id));

    public Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        => Collection.ReplaceOneAsync(predicate, entity);

    public Task DeleteAsync(TIdentifiable id)
        => DeleteAsync(e => e.Id.Equals(id));

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        => Collection.DeleteOneAsync(predicate);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        => Collection.Find(predicate).AnyAsync();

    public IQueryable<TEntity> GetAll()
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        throw new NotImplementedException();
    }

    public List<TEntity> GetAllList()
    {
        throw new NotImplementedException();
    }

    public Task<List<TEntity>> GetAllListAsync()
    {
        throw new NotImplementedException();
    }

    public List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public T Query<T>(Func<IQueryable<TEntity>, T> queryMethod)
    {
        throw new NotImplementedException();
    }

    public TEntity Get(TIdentifiable id)
    {
        throw new NotImplementedException();
    }

    public TEntity Single(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public TEntity FirstOrDefault(TIdentifiable id)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> FirstOrDefaultAsync(TIdentifiable id)
    {
        throw new NotImplementedException();
    }

    public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public TEntity Load(TIdentifiable id)
    {
        throw new NotImplementedException();
    }

    public TEntity Insert(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> InsertAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public TIdentifiable1 InsertAndGetId<TIdentifiable1>(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<TIdentifiable1> InsertAndGetIdAsync<TIdentifiable1>(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public TEntity InsertOrUpdate(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> InsertOrUpdateAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public TIdentifiable1 InsertOrUpdateAndGetId<TIdentifiable1>(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<TIdentifiable1> InsertOrUpdateAndGetIdAsync<TIdentifiable1>(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public TEntity Update(TEntity entity)
    {
        throw new NotImplementedException();
    }

    Task<TEntity> IRepositoryOfEntity<TEntity, TIdentifiable>.UpdateAsync(TEntity entity)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public Task DeleteAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(TIdentifiable id)
    {
        throw new NotImplementedException();
    }

    public void Delete(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public int Count()
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync()
    {
        throw new NotImplementedException();
    }

    public int Count(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public long LongCount()
    {
        throw new NotImplementedException();
    }

    public Task<long> LongCountAsync()
    {
        throw new NotImplementedException();
    }

    public long LongCount(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }
}