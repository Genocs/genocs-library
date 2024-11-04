using Genocs.Core.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Repositories.Clean;

/// <summary>
/// Default MongoDB entity.
/// </summary>
public interface IMongoDbEntity : IEntity<ObjectId>
{

}
