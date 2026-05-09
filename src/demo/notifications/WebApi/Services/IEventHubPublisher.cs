using Genocs.Notifications.WebApi.Messages.Events;

namespace Genocs.Notifications.WebApi.Services;

public interface IEventHubPublisher
{
    Task PublishOrderCreatedAsync(OrderCreated @event, CancellationToken cancellationToken = default);
}