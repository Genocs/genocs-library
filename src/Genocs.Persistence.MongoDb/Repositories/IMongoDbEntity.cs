using Genocs.Core.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Repositories;

/// <summary>
/// General purpose Entity used by default in MongoDB
/// </summary>
public interface IMongoDbEntity : IEntity<ObjectId>
{

}
