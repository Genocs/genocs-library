using Genocs.Common.CQRS.Events;
using Genocs.Notifications.WebApi.Messages.Events;
using Genocs.Notifications.WebApi.Services;

namespace Genocs.Notifications.WebApi.Handlers;

public class OrderCreatedHandler(IEventHubPublisher eventHubPublisher, IHubService hubService, ILogger<OrderCreatedHandler> logger) : IEventHandler<OrderCreated>
{
    private readonly IEventHubPublisher _eventHubPublisher = eventHubPublisher ?? throw new ArgumentNullException(nameof(eventHubPublisher));
    private readonly IHubService _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
    private readonly ILogger<OrderCreatedHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task HandleAsync(OrderCreated message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received order created event for order '{OrderId}'.", message.OrderId);
        await _hubService.PublishOrderCreatedAsync(message);
        await _eventHubPublisher.PublishOrderCreatedAsync(message, cancellationToken);
    }
}