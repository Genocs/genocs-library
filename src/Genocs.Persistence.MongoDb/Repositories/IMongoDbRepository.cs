using Genocs.Common.Types;

namespace Genocs.Persistence.MongoDb.Repositories;


/// <summary>
/// The MongoDb repository interface 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IMongoDbRepository<TEntity> : IMongoRepository<TEntity, Guid> where TEntity : IIdentifiable<Guid>
{

}
