using Genocs.Common.CQRS.Events;
using Genocs.Library.Demo.Contracts;

namespace Genocs.Library.Demo.Masstransit.Worker.Handlers;

public class OrderRequestEventHandler : IEventHandler<OrderRequest>
{
    private readonly ILogger<OrderRequestEventHandler> _logger;

    public OrderRequestEventHandler(ILogger<OrderRequestEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task HandleAsync(OrderRequest context, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"OrderRequest '{0}', '{1}' processed", context.OrderId, context.UserId);

        // Do something with the message here
        return Task.CompletedTask;
    }
}
