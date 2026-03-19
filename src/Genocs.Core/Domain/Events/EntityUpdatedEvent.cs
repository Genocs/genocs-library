using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Entities;

namespace Genocs.Core.Domain.Events;

/// <summary>
/// Represents an event that is raised when an entity is updated.
/// </summary>
public static class EntityUpdatedEvent
{
    public static EntityUpdatedEvent<TEntity> WithEntity<TEntity>(TEntity entity)
        where TEntity : IEntity
        => new(entity);
}

/// <summary>
/// Represents an event that is raised when an entity is updated.
/// </summary>
/// <typeparam name="TEntity">The type of the entity that was updated.</typeparam>
public class EntityUpdatedEvent<TEntity> : DomainEvent
    where TEntity : IEntity
{
    internal EntityUpdatedEvent(TEntity entity)
        => Entity = entity;

    /// <summary>
    /// Gets the entity that was updated.
    /// </summary>
    public TEntity Entity { get; }
}