using Genocs.Core.Demo.Contracts;
using Genocs.Core.Interfaces;

namespace Genocs.Core.Demo.ServiceBusAzure.Worker.Handlers;

public class OrderRequestEventHandler : IEventHandler<OrderRequest>
{
    private readonly ILogger<OrderRequestEventHandler> _logger;

    public OrderRequestEventHandler(ILogger<OrderRequestEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task HandleEvent(OrderRequest context)
    {
        _logger.LogInformation($"OrderRequest '{0}', '{1}' processed", context.OrderId, context.UserId);

        // Do something with the message here
        return Task.CompletedTask;
    }
}
