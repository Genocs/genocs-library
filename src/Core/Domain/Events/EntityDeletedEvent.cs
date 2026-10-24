using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Entities;

namespace Genocs.Core.Domain.Events;

/// <summary>
/// Represents an event that is raised when an entity is deleted.
/// </summary>
public static class EntityDeletedEvent
{
    public static EntityDeletedEvent<TEntity> WithEntity<TEntity>(TEntity entity)
        where TEntity : IEntity
        => new(entity);
}

/// <summary>
/// Represents an event that is raised when an entity is deleted.
/// </summary>
/// <typeparam name="TEntity">The type of the entity that was deleted.</typeparam>
public class EntityDeletedEvent<TEntity> : DomainEvent
    where TEntity : IEntity
{
    internal EntityDeletedEvent(TEntity entity)
        => Entity = entity;

    /// <summary>
    /// Gets the entity that was deleted.
    /// </summary>
    public TEntity Entity { get; }
}
