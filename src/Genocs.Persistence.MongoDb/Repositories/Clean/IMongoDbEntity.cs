using Genocs.Common.Types;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Repositories.Clean;

/// <summary>
/// General purpose Entity used by default in MongoDB.
/// </summary>
public interface IMongoDbEntity : IIdentifiable<ObjectId>
{

}
