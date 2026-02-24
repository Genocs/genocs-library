using System.ComponentModel.DataAnnotations.Schema;
using Genocs.Common.Cqrs.Events;
using Genocs.Common.Domain.Entities;

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