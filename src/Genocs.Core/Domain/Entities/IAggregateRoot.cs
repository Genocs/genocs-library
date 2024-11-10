// using System.Collections.Generic;
// using Genocs.Events.Bus;

namespace Genocs.Core.Domain.Entities;

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
