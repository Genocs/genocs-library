using Genocs.Common.Types;

namespace Genocs.Persistence.MongoDb.Repositories.Clean;

/// <summary>
/// General purpose Entity used by default in MongoDB.
/// </summary>
public interface IMongoDbEntity : IIdentifiable<Guid>
{

}
