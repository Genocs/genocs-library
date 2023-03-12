namespace Genocs.Persistence.MongoDb.Repositories
{
    using Genocs.Core.CQRS.Queries;
    using Genocs.Core.Domain.Entities;
    using Genocs.Core.Domain.Repositories;
    using MongoDB.Bson;
    using MongoDB.Driver.Linq;
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IMongoDbRepository<TEntity> : IRepository<TEntity, ObjectId> where TEntity : class, IEntity<ObjectId>
    {
        /// <summary>
        /// Todo
        /// </summary>
        /// <returns></returns>
        IMongoQueryable<TEntity> GetMongoQueryable();


        /// <summary>
        /// todo
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate, TQuery query) where TQuery : PagedQueryBase;
    }
}
