using Genocs.Common.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDb.Domain.Entities;

/// <summary>
/// Default MongoDB entity.
/// </summary>
public interface IMongoDbEntity : IEntity<ObjectId>;
