// using System.Collections.Generic;
// using Genocs.Events.Bus;

namespace Genocs.Core.Domain.Entities;

/// <summary>
/// Apply this marker interface only to aggregate root entities
/// Repositories will only work with aggregate roots, not their children.
/// </summary>
public interface IAggregateRoot : IEntity
{

}

public interface IAggregateRoot<TKey> : IEntity<TKey>, IAggregateRoot/*, IGeneratesDomainEvents */
{

}

/*
public interface IGeneratesDomainEvents
{
    ICollection<IEventData> DomainEvents { get; }
}

*/
