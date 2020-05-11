namespace Genocs.Core.Domain.Entities
{
    //using System.Collections.Generic;
    //using System.Collections.ObjectModel;
    //using System.ComponentModel.DataAnnotations.Schema;
    //using Genocs.Events.Bus;

    public class AggregateRoot : AggregateRoot<int>, IAggregateRoot
    {

    }

    public class AggregateRoot<TPrimaryKey> : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
    {
        /*
        [NotMapped]
        public virtual ICollection<IEventData> DomainEvents { get; }

        public AggregateRoot()
        {
            DomainEvents = new Collection<IEventData>();
        }
        */
    }
}