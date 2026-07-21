using Genocs.Common.CQRS.Events;
using Genocs.Library.Demo.Contracts;

namespace Genocs.Library.Demo.ServiceBus.Worker.Handlers;

public class TransactionCompletedHandler(ILogger<TransactionCompletedHandler> logger) : IEventHandler<TransactionCompleted>
{
    private readonly ILogger<TransactionCompletedHandler> _logger = logger;

    public Task HandleAsync(TransactionCompleted @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Received '{nameof(TransactionCompleted)}' event with text: {@event.Text}");
        return Task.CompletedTask;
    }
}