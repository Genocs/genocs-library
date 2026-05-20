using System.ComponentModel.DataAnnotations.Schema;
using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;

namespace Genocs.Core.Domain.Entities;

public class AggregateRoot : AggregateRoot<DefaultIdType>, IAggregateRoot;

public class AggregateRoot<TPrimaryKey>
    : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
{
    [NotMapped]
    public virtual List<IEvent> DomainEvents { get; } = [];

    IReadOnlyCollection<IEvent> IGeneratesDomainEvents.DomainEvents => DomainEvents;

    public void AddDomainEvent(IEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        DomainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
    }
}