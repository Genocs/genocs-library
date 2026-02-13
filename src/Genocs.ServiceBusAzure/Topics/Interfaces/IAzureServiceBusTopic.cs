using Genocs.Common.CQRS.Events;
using Genocs.Core.CQRS.Events;

namespace Genocs.ServiceBusAzure.Topics.Interfaces;

/// <summary>
/// Todo
/// </summary>
public interface IAzureServiceBusTopic
{
    Task PublishAsync(IEvent @event);
    Task PublishAsync(IEvent @event, Dictionary<string, object> filters);
    Task ScheduleAsync(IEvent @event, DateTimeOffset offset);
    Task ScheduleAsync(IEvent @event, DateTimeOffset offset, Dictionary<string, object> filters);
    void Subscribe<T, TH>() where T : IEvent where TH : IEventHandlerLegacy<T>;
}
