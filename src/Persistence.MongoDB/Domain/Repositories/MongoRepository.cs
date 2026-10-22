using Genocs.Persistence.MongoDB.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDB.Domain.Repositories;

/// <summary>
/// Implements IRepository for MongoDB.
/// </summary>
/// <typeparam name="TEntity">Type of the Entity for this repository.</typeparam>
/// <remarks>
/// The standard constructor.
/// </remarks>
/// <param name="databaseProvider">The database provider.</param>
public class MongoRepository<TEntity>(IMongoDatabaseProvider databaseProvider) : MongoBaseRepositoryOfType<TEntity, ObjectId>(databaseProvider), IMongoRepository<TEntity>
    where TEntity : IMongoEntity
{
}