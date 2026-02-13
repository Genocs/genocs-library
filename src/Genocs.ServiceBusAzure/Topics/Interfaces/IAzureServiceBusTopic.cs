using Genocs.Common.CQRS.Events;

namespace Genocs.ServiceBusAzure.Topics.Interfaces;

/// <summary>
/// This interface defines the contract for an Azure Service Bus Topic, providing methods for publishing events and subscribing to event handlers.
/// </summary>
public interface IAzureServiceBusTopic
{
    /// <summary>
    /// Asynchronously publishes the specified event to the event bus.
    /// </summary>
    /// <remarks>This method is intended for scenarios where events should be published without blocking the
    /// calling thread. Ensure that the event is properly configured before invoking this method.</remarks>
    /// <param name="event">The event to be published. This parameter cannot be null and must implement the IEvent interface.</param>
    /// <returns>A task that represents the asynchronous operation of publishing the event.</returns>
    Task PublishAsync(IEvent @event);

    Task PublishAsync(IEvent @event, Dictionary<string, object> filters);

    Task ScheduleAsync(IEvent @event, DateTimeOffset offset);

    Task ScheduleAsync(IEvent @event, DateTimeOffset offset, Dictionary<string, object> filters);

    void Subscribe<T, TH>()
        where T : IEvent
        where TH : IEventHandlerLegacy<T>;
}
