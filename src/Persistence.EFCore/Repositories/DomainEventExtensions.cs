using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;

namespace Genocs.Persistence.EFCore.Repositories;

public static class DomainEventExtensions
{
    /// <summary>
    /// Adds a domain event through the aggregate's explicit contract.
    /// </summary>
    public static void AddDomainEvent(this IGeneratesDomainEvents aggregate, IEvent @event)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        aggregate.AddDomainEvent(@event);
    }
}
