using Genocs.Core.CQRS.Events;

namespace Genocs.Core.Domain.Entities;

/// <summary>
/// Apply this marker interface only to aggregate root entities
/// Repositories will only work with aggregate roots, not their children.
/// </summary>
public interface IAggregateRoot : IEntity;

public interface IAggregateRoot<TKey> : IEntity<TKey>, IAggregateRoot, IGeneratesDomainEvents;

/// <summary>
/// This interface is used to identify a domain event source.
/// </summary>
public interface IGeneratesDomainEvents
{
    List<IEvent>? DomainEvents { get; }
}
