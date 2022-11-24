namespace Genocs.Persistence.MongoDb.Repositories
{
    using Genocs.Core.Base;
    using Genocs.Core.Domain.Entities;
    using Genocs.Core.Domain.Repositories;
    using Genocs.Persistence.MongoDb;
    using Genocs.Persistence.MongoDb.Repositories;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;


    /// <summary>
    /// Implements IRepository for MongoDB.
    /// </summary>
    /// <typeparam name="TEntity">Type of the Entity for this repository</typeparam>
    public class MongoDbRepository<TEntity> : MongoDbRepositoryBase<TEntity> where TEntity : class, IMongoDbEntity
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

    /// <summary>
    /// Implements IRepository for MongoDB.
    /// </summary>
    /// <typeparam name="TEntity">Type of the Entity for this repository</typeparam>
    public class MongoDbRepositoryBase<TEntity> : MongoDbRepositoryBase<TEntity, ObjectId>, IMongoDbRepository<TEntity>
        where TEntity : class, IEntity<ObjectId>
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
        /// 
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
            TQuery query) where TQuery : PagedQueryBase
                => await Collection.AsQueryable().Where(predicate).PaginateAsync(query);
    }


    /// <summary>
    /// Implements IRepository for MongoDB.
    /// </summary>
    /// <typeparam name="TEntity">Type of the Entity for this repository</typeparam>
    /// <typeparam name="TPrimaryKey">Primary key of the entity</typeparam>
    public class MongoDbRepositoryBase<TEntity, TPrimaryKey> : RepositoryBase<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        /// <summary>
        /// Todo
        /// </summary>
        public virtual IMongoDatabase Database
        {
            get { return _databaseProvider.Database; }
        }

        /// <summary>
        /// Todo
        /// </summary>
        public virtual IMongoCollection<TEntity> Collection
        {
            get
            {
                Attribute[] attrs = Attribute.GetCustomAttributes(typeof(TEntity));  // Reflection.  

                // Displaying output.  
                foreach (Attribute attr in attrs)
                {
                    if (attr is TableMappingAttribute)
                    {
                        return _databaseProvider.Database.GetCollection<TEntity>((attr as TableMappingAttribute).Name);
                    }
                }
                return _databaseProvider.Database.GetCollection<TEntity>(typeof(TEntity).Name);
            }
        }

        private readonly IMongoDatabaseProvider _databaseProvider;

        public MongoDbRepositoryBase(IMongoDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        public override IQueryable<TEntity> GetAll()
        {
            return Collection.AsQueryable();
        }

        public override TEntity Get(TPrimaryKey id)
        {
            FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq(m => m.Id, id);
            var entity = Collection.Find(filter).FirstOrDefault();
            if (entity == null)
            {
                throw new EntityNotFoundException("There is no such an entity with given primary key. Entity type: " + typeof(TEntity).FullName + ", primary key: " + id);
            }

            return entity;
        }

        public override TEntity FirstOrDefault(TPrimaryKey id)
        {
            FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq(m => m.Id, id);
            return Collection.Find(filter).FirstOrDefault();
        }

        public override TEntity Insert(TEntity entity)
        {
            Collection.InsertOne(entity);
            return entity;
        }

        public override TEntity Update(TEntity entity)
        {
            Collection.ReplaceOneAsync(
            filter: g => g.Id.Equals(entity.Id),
            replacement: entity);
            return entity;
        }

        public override void Delete(TEntity entity)
        {
            Delete(entity.Id);
        }

        public override void Delete(TPrimaryKey id)
        {
            FilterDefinition<TEntity> query = Builders<TEntity>.Filter.Eq(m => m.Id, id);
            DeleteResult deleteResult = Collection.DeleteOneAsync(query).Result;
        }
    }
}