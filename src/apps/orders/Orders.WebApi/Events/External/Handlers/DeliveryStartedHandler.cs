using Genocs.Core.CQRS.Events;

namespace Genocs.Orders.WebApi.Events.External.Handlers;

public class DeliveryStartedHandler : IEventHandler<DeliveryStarted>
{
    private readonly ILogger<DeliveryStartedHandler> _logger;

    public DeliveryStartedHandler(ILogger<DeliveryStartedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(DeliveryStarted @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Received 'delivery started' event with delivery id: {@event.DeliveryId}");
        return Task.CompletedTask;
    }
}