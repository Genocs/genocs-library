using Genocs.Common.Domain.Entities.Auditing;
using Genocs.Core.Domain.Entities;
using Genocs.Persistence.MongoDB.Domain.Entities;
using MongoDB.Bson;

namespace Genocs.Core.Demo.Domain.Aggregates;

/// <summary>
/// Base aggregate class used for all entities.
/// This class is used to define some common properties for all entities.
/// </summary>
public class BaseAggregate : AggregateRoot<ObjectId>, IMongoEntity, IHasCreationTime
{
    public BaseAggregate()
    {
        // Set the unique identifier for the entity generates a new ObjectId.
        Id = ObjectId.GenerateNewId();
    }

    /// <summary>
    /// Creation time of this entity.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}