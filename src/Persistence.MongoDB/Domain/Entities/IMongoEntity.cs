using Genocs.Common.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Persistence.MongoDB.Domain.Entities;

/// <summary>
/// Default MongoDB entity.
/// </summary>
public interface IMongoEntity : IEntity<ObjectId>;
