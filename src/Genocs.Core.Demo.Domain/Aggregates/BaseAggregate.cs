using Genocs.Core.Domain.Entities;
using Genocs.Core.Domain.Entities.Auditing;
using Genocs.Persistence.MongoDb.Repositories.Clean;
using MongoDB.Bson;

namespace Genocs.Core.Demo.Domain.Aggregates;

/// <summary>
/// Base aggregate class used for all entities.
/// </summary>
public class BaseAggregate : AggregateRoot<ObjectId>, IMongoDbEntity, IHasCreationTime
{
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}