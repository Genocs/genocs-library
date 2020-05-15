namespace Genocs.Persistence.MongoDb.Repository
{
    using Genocs.Core.Base;
    using Genocs.Core.Domain.Entities;
    using Genocs.Core.Domain.Repositories;
    using MongoDB.Bson;
    using MongoDB.Driver.Linq;
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IMongoDbRepository<TEntity> : IRepository<TEntity, ObjectId> where TEntity : class, IEntity<ObjectId>
    {
        IMongoQueryable<TEntity> GetMongoQueryable();
        Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate, TQuery query) where TQuery : PagedQueryBase;
    }
}
