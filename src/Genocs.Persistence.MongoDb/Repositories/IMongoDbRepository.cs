using Genocs.Common.Types;
using Genocs.Persistence.MongoDb.Repositories.Mentor;

namespace Genocs.Persistence.MongoDb.Repositories;

/// <summary>
/// The MongoDb repository interface.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IMongoDbRepository<TEntity> : IMongoRepository<TEntity, Guid>
    where TEntity : IIdentifiable<Guid>
{

}
