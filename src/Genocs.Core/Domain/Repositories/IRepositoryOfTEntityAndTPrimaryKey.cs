using Genocs.Core.Domain.Entities;
using System.Linq.Expressions;

namespace Genocs.Core.Domain.Repositories;

/// <summary>
/// This interface is implemented by all repositories to ensure implementation of fixed methods.
/// </summary>
/// <typeparam name="TEntity">Main Entity type this repository works on.</typeparam>
/// <typeparam name="TKey">Primary key type of the entity.</typeparam>
public interface IRepositoryOfEntity<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    #region Select/Get/Query

    /// <summary>
    /// Used to get a IQueryable that is used to retrieve entities from entire table.
    /// </summary>
    /// <returns>IQueryable to be used to select entities from database.</returns>
    IQueryable<TEntity> GetAll();

    /// <summary>
    /// Used to get a IQueryable that is used to retrieve entities from entire table.
    /// One or more.
    /// </summary>
    /// <param name="propertySelectors">A list of include expressions.</param>
    /// <returns>IQueryable to be used to select entities from database.</returns>
    IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    /// Used to get all entities.
    /// </summary>
    /// <returns>List of all entities.</returns>
    List<TEntity> GetAllList();

    /// <summary>
    /// Used to get all entities.
    /// </summary>
    /// <returns>List of all entities.</returns>
    Task<List<TEntity>> GetAllListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Used to get all entities based on given <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">A condition to filter entities.</param>
    /// <returns>List of all entities.</returns>
    List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Used to get all entities based on given <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">A condition to filter entities.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of all entities.</returns>
    Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Used to run a query over entire entities.
    /// if <paramref name="queryMethod"/> finishes IQueryable with ToList, FirstOrDefault etc..
    /// </summary>
    /// <typeparam name="T">Type of return value of this method.</typeparam>
    /// <param name="queryMethod">This method is used to query over entities.</param>
    /// <returns>Query result.</returns>
    T Query<T>(Func<IQueryable<TEntity>, T> queryMethod);

    /// <summary>
    /// Gets an entity with given primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity to get.</param>
    /// <returns>Entity.</returns>
    TEntity Get(TKey id);

    /// <summary>
    /// Gets an entity with given primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity to get.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Entity.</returns>
    Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets exactly one entity with given predicate.
    /// Throws exception if no entity or more than one entity.
    /// </summary>
    /// <param name="predicate">Entity.</param>
    TEntity Single(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Gets exactly one entity with given predicate.
    /// Throws exception if no entity or more than one entity.
    /// </summary>
    /// <param name="predicate">Entity.</param>
    /// <param name="cancellationToken"></param>
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity with given primary key or null if not found.
    /// </summary>
    /// <param name="id">Primary key of the entity to get.</param>
    /// <returns>Entity or null.</returns>
    TEntity? FirstOrDefault(TKey id);

    /// <summary>
    /// Gets an entity with given primary key or null if not found.
    /// </summary>
    /// <param name="id">Primary key of the entity to get.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Entity or null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity with given predicate or null if not found.
    /// </summary>
    /// <param name="predicate">Predicate to filter entities.</param>
    TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Gets an entity with given predicate or null if not found.
    /// </summary>
    /// <param name="predicate">Predicate to filter entities.</param>
    /// <param name="cancellationToken"></param>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an entity with given primary key without database access.
    /// </summary>
    /// <param name="id">Primary key of the entity to load.</param>
    /// <returns>Entity.</returns>
    TEntity? Load(TKey id);

    #endregion

    #region Insert

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    /// <param name="entity">Inserted entity.</param>
    TEntity Insert(TEntity entity);

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    /// <param name="entity">Inserted entity.</param>
    /// <param name="cancellationToken"></param>
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new entity and gets it's Id.
    /// It may require to save current unit of work
    /// to be able to retrieve id.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <returns>Id of the entity.</returns>
    TKey InsertAndGetId(TEntity entity);

    /// <summary>
    /// Inserts a new entity and gets it's Id.
    /// It may require to save current unit of work
    /// to be able to retrieve id.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Id of the entity.</returns>
    Task<TKey> InsertAndGetIdAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates given entity depending on Id's value.
    /// </summary>
    /// <param name="entity">Entity.</param>
    TEntity InsertOrUpdate(TEntity entity);

    /// <summary>
    /// Inserts or updates given entity depending on Id's value.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="cancellationToken"></param>
    Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates given entity depending on Id's value.
    /// Also returns Id of the entity.
    /// It may require to save current unit of work
    /// to be able to retrieve id.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <returns>Id of the entity.</returns>
    TKey InsertOrUpdateAndGetId(TEntity entity);

    /// <summary>
    /// Inserts or updates given entity depending on Id's value.
    /// Also returns Id of the entity.
    /// It may require to save current unit of work
    /// to be able to retrieve id.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Id of the entity.</returns>
    Task<TKey> InsertOrUpdateAndGetIdAsync(TEntity entity, CancellationToken cancellationToken = default);

    #endregion

    #region Update

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">Entity.</param>
    TEntity Update(TEntity entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="cancellationToken"></param>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="id">Id of the entity.</param>
    /// <param name="updateAction">Action that can be used to change values of the entity.</param>
    /// <returns>Updated entity.</returns>
    TEntity Update(TKey id, Action<TEntity> updateAction);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="id">Id of the entity.</param>
    /// <param name="updateAction">Action that can be used to change values of the entity.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Updated entity.</returns>
    Task<TEntity> UpdateAsync(TKey id, Func<TEntity, Task> updateAction, CancellationToken cancellationToken);

    #endregion

    #region Delete

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">Entity to be deleted.</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">Entity to be deleted.</param>
    /// <param name="cancellationToken"></param>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an entity by primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity.</param>
    void Delete(TKey id);

    /// <summary>
    /// Deletes an entity by primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity.</param>
    /// <param name="cancellationToken"></param>
    Task DeleteAsync(TKey id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes many entities by function.
    /// Notice that: All entities fits to given predicate are retrieved and deleted.
    /// This may cause major performance problems if there are too many entities with
    /// given predicate.
    /// </summary>
    /// <param name="predicate">A condition to filter entities.</param>
    void Delete(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Deletes many entities by function.
    /// Notice that: All entities fits to given predicate are retrieved and deleted.
    /// This may cause major performance problems if there are too many entities with
    /// given predicate.
    /// </summary>
    /// <param name="predicate">A condition to filter entities.</param>
    /// <param name="cancellationToken"></param>
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    #endregion

    #region Aggregates

    /// <summary>
    /// Gets count of all entities in this repository.
    /// </summary>
    /// <returns>Count of entities.</returns>
    int Count();

    /// <summary>
    /// Gets count of all entities in this repository.
    /// </summary>
    /// <returns>Count of entities.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets count of all entities in this repository based on given <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">A method to filter count.</param>
    /// <returns>Count of entities.</returns>
    int Count(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Gets count of all entities in this repository based on given <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">A method to filter count.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Count of entities.</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    /// <summary>
    /// Gets count of all entities in this repository (use if expected return value is greater than <see cref="int.MaxValue"/>.
    /// </summary>
    /// <returns>Count of entities.</returns>
    long LongCount();

    /// <summary>
    /// Gets count of all entities in this repository (use if expected return value is greater than <see cref="int.MaxValue"/>.
    /// </summary>
    /// <returns>Count of entities.</returns>
    Task<long> LongCountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets count of all entities in this repository based on given <paramref name="predicate"/>
    /// (use this overload if expected return value is greater than <see cref="int.MaxValue"/>).
    /// </summary>
    /// <param name="predicate">A method to filter count.</param>
    /// <returns>Count of entities.</returns>
    long LongCount(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Gets count of all entities in this repository based on given <paramref name="predicate"/>
    /// (use this overload if expected return value is greater than <see cref="int.MaxValue"/>).
    /// </summary>
    /// <param name="predicate">A method to filter count.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Count of entities.</returns>
    Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    #endregion
}
