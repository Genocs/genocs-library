using Genocs.Common.CQRS.Events;

namespace Genocs.Core.Domain.Entities;

/// <summary>
/// Base class for all domain events.
/// </summary>
public abstract class DomainEvent : IEvent
{

    /// <summary>
    /// The date and time on which the event was triggered.
    /// </summary>
    public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
}