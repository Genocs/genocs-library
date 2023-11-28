using Genocs.Common.Types;
using Genocs.Core.CQRS.Queries;
using Genocs.Core.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.MongoDb.Repositories.Mentor;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace Genocs.Persistence.MongoDb.Repositories.Clean;

/// <summary>
/// Implements IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository.</typeparam>
/// <typeparam name="TPrimaryKey">Primary key of the entity.</typeparam>
public class MongoDbRepositoryBase<TEntity, TPrimaryKey> : RepositoryBase<TEntity, TPrimaryKey>, IMongoRepository<TEntity, TPrimaryKey>
    where TEntity : class, IIdentifiable<TPrimaryKey>
{
    /// <summary>
    /// Get the Mongodb database.
    /// </summary>
    public virtual IMongoDatabase Database
    {
        get { return _databaseProvider.Database; }
    }

    private readonly IMongoDatabaseProvider _databaseProvider;
    protected IMongoCollection<TEntity>? _collection;

    /// <summary>
    /// Get the Mongodb collection from a custom attribute or from the entity name.
    /// </summary>
    public virtual IMongoCollection<TEntity> Collection
    {
        get
        {
            if (_collection != null)
            {
                return _collection;
            }

            var attrs = Attribute.GetCustomAttributes(typeof(TEntity));  // Reflection.

            // Displaying output.
            foreach (var attr in attrs)
            {
                if (attr is TableMappingAttribute)
                {
                    return _databaseProvider.Database.GetCollection<TEntity>((attr as TableMappingAttribute).Name);
                }
            }

            _collection = _databaseProvider.Database.GetCollection<TEntity>(typeof(TEntity).Name);

            return _collection;
        }
    }

    /// <summary>
    /// Standard constructor.
    /// </summary>
    /// <param name="databaseProvider"></param>
    public MongoDbRepositoryBase(IMongoDatabaseProvider databaseProvider)
    {
        _databaseProvider = databaseProvider;
    }

    /// <summary>
    /// Get all entities as IQueryable.
    /// </summary>
    /// <returns></returns>
    public override IQueryable<TEntity> GetAll()
        => Collection.AsQueryable();

    /// <summary>
    /// Get single entity.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException">It is thrown if the entity is not found.</exception>
    public override TEntity Get(TPrimaryKey id)
    {
        var filter = Builders<TEntity>.Filter.Eq(m => m.Id, id);
        var entity = Collection.Find(filter).FirstOrDefault();
        if (entity == null)
        {
            throw new EntityNotFoundException("There is no such an entity with given primary key. Entity type: " + typeof(TEntity).FullName + ", primary key: " + id);
        }

        return entity;
    }

    /// <summary>
    /// First Or Default entity.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override TEntity FirstOrDefault(TPrimaryKey id)
    {
        var filter = Builders<TEntity>.Filter.Eq(m => m.Id, id);
        return Collection.Find(filter).FirstOrDefault();
    }

    /// <summary>
    /// Insert an entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override TEntity Insert(TEntity entity)
    {
        Collection.InsertOne(entity);
        return entity;
    }

    /// <summary>
    /// Update an existing entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override TEntity Update(TEntity entity)
    {
        Collection.ReplaceOneAsync(filter: g => g.Id.Equals(entity.Id), replacement: entity);
        return entity;
    }

    /// <summary>
    /// Delete entity, passing the entire object.
    /// </summary>
    /// <param name="entity"></param>
    public override void Delete(TEntity entity)
        => Delete(entity.Id);

    /// <summary>
    /// Delete entity by primary key.
    /// </summary>
    /// <param name="id"></param>
    public override void Delete(TPrimaryKey id)
    {
        var query = Builders<TEntity>.Filter.Eq(m => m.Id, id);
        var deleteResult = Collection.DeleteOneAsync(query).Result;
    }

    public IMongoQueryable<TEntity> GetMongoQueryable()
        => Collection.AsQueryable();

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
        => await Collection.AsQueryable().Where(predicate).FirstAsync();

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        => await Collection.AsQueryable().Where(predicate).ToListAsync();

    public async Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate, TQuery query)
        where TQuery : IPagedQuery
        => await Collection.AsQueryable().Where(predicate).PaginateAsync(query);

    public async Task AddAsync(TEntity entity)
        => await Collection.InsertOneAsync(entity);

    public async Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        => await Collection.ReplaceOneAsync(predicate, entity);

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        => await Collection.AsQueryable().Where(predicate).AnyAsync();


    //public Task<TIdentifiable> InsertAndGetIdAsync<TIdentifiable>(TEntity entity)
    //{
    //    throw new NotImplementedException();
    //}

    //public TIdentifiable InsertOrUpdateAndGetId<TIdentifiable>(TEntity entity)
    //{
    //    throw new NotImplementedException();
    //}

    //public Task<TIdentifiable> InsertOrUpdateAndGetIdAsync<TIdentifiable>(TEntity entity)
    //{
    //    throw new NotImplementedException();
    //}
}