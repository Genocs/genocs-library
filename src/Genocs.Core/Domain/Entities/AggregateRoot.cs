using System.ComponentModel.DataAnnotations.Schema;
using Genocs.Core.CQRS.Events;

namespace Genocs.Core.Domain.Entities;

public class AggregateRoot : AggregateRoot<DefaultIdType>, IAggregateRoot;

public class AggregateRoot<TPrimaryKey>
    : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
{
    [NotMapped]
    public virtual List<IEvent>? DomainEvents { get; }

    public AggregateRoot()
    {
        DomainEvents = [];
    }
}