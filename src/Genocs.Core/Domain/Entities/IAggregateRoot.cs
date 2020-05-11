namespace Genocs.Core.Domain.Entities
{
    //using System.Collections.Generic;
    //using Genocs.Events.Bus;

    public interface IAggregateRoot : IAggregateRoot<int>, IEntity
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
}