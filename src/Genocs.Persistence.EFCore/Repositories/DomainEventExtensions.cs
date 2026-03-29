using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;

namespace Genocs.Persistence.EFCore.Repositories;

public static class DomainEventExtensions
{
    /// <summary>
    /// Adds a domain event to the aggregate root's internal event collection.
    /// </summary>
    public static void AddDomainEvent(this IGeneratesDomainEvents aggregate, IEvent @event)
    {
        // This assumes the actual implementation of IGeneratesDomainEvents
        // uses a private List<IEvent> and exposes it as IReadOnlyCollection<IEvent>.
        // Use reflection to add to the private list if necessary.
        var field = aggregate.GetType().GetField("_domainEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(aggregate) is List<IEvent> list)
        {
            list.Add(@event);
        }
        else
        {
            throw new InvalidOperationException("Cannot add domain event: backing field not found or not a List<IEvent>.");
        }
    }
}
