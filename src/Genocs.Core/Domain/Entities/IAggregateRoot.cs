// using System.Collections.Generic;
// using Genocs.Events.Bus;

namespace Genocs.Core.Domain.Entities;

public interface IAggregateRoot : IAggregateRoot<Guid>, IEntity
{

}

public interface IAggregateRoot<TPrimaryKey> : IEntity<TPrimaryKey>/*, IGeneratesDomainEvents */
{

}

/*
public interface IGeneratesDomainEvents
{
    ICollection<IEventData> DomainEvents { get; }
}

*/
