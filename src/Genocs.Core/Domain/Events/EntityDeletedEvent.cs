using Genocs.Core.Domain.Entities;

namespace Genocs.Core.Domain.Events;

public static class EntityDeletedEvent
{
    public static EntityDeletedEvent<TEntity> WithEntity<TEntity>(TEntity entity)
        where TEntity : IEntity
        => new(entity);
}

public class EntityDeletedEvent<TEntity> : DomainEvent
    where TEntity : IEntity
{
    internal EntityDeletedEvent(TEntity entity)
        => Entity = entity;

    public TEntity Entity { get; }
}
