using Genocs.Common.CQRS.Events;

namespace Genocs.Messaging.AzureServiceBus.Topics.Interfaces;

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

    /// <summary>
    /// Registers a modern event handler contract for topic subscriptions.
    /// </summary>
    void SubscribeModern<T, TH>()
        where T : class, IEvent
        where TH : IEventHandler<T>;

    /// <summary>
    /// Registers a legacy event handler contract for topic subscriptions.
    /// </summary>
    [Obsolete("Subscribe<T,TH>() uses legacy IEventHandlerLegacy<T>. Use SubscribeModern<T,TH>() with IEventHandler<T>. Legacy registration will be removed in a future major release.")]
    void Subscribe<T, TH>()
        where T : IEvent
        where TH : IEventHandlerLegacy<T>;
}
