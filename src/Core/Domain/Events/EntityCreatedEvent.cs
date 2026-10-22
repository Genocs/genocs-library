using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Entities;

namespace Genocs.Core.Domain.Events;

/// <summary>
/// Provides static factory methods for creating instances of the EntityCreatedEvent class for entities that implement
/// the IEntity interface.
/// </summary>
/// <remarks>This class is intended to simplify the creation of entity creation events by ensuring type safety and
/// consistency. It is commonly used in domain-driven design patterns to signal that a new entity has been
/// created.</remarks>
public static class EntityCreatedEvent
{
    /// <summary>
    /// Creates a new EntityCreatedEvent instance that encapsulates the specified entity.
    /// </summary>
    /// <remarks>Use this method to generate an event representing the creation of an entity within the
    /// domain. This is typically used in event-driven or domain-driven design scenarios to signal that a new entity has
    /// been added to the system.</remarks>
    /// <typeparam name="TEntity">The type of the entity associated with the event. Must implement the IEntity interface.</typeparam>
    /// <param name="entity">The entity to associate with the created event. This parameter cannot be null.</param>
    /// <returns>An EntityCreatedEvent&lt;TEntity&gt; that contains the provided entity.</returns>
    public static EntityCreatedEvent<TEntity> WithEntity<TEntity>(TEntity entity)
        where TEntity : IEntity
        => new(entity);
}

/// <summary>
/// Represents a domain event that occurs when a new entity of type TEntity is created.
/// </summary>
/// <remarks>This event can be used in event-driven architectures to notify subscribers when a new entity instance
/// is added to the domain. It is typically published by repositories or aggregate roots after a successful creation
/// operation.</remarks>
/// <typeparam name="TEntity">The type of the entity that was created. This type must implement the IEntity interface.</typeparam>
public class EntityCreatedEvent<TEntity> : DomainEvent
    where TEntity : IEntity
{
    internal EntityCreatedEvent(TEntity entity)
        => Entity = entity;

    /// <summary>
    /// Gets the entity associated with the current context.
    /// </summary>
    /// <remarks>Accessing this property provides the underlying entity of type TEntity. Ensure that the
    /// context is properly initialized before accessing this property to avoid unexpected behavior.</remarks>
    public TEntity Entity { get; }
}