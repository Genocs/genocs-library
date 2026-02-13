using Genocs.Common.CQRS.Events;
using Genocs.Library.Demo.Contracts;

namespace Genocs.Library.Demo.Worker.Handlers;

public class DemoEventHandler : IEventHandler<DemoEvent>
{
    private readonly ILogger<DemoEventHandler> _logger;

    public DemoEventHandler(ILogger<DemoEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(DemoEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"DemoEvent '{@event.Name}' processed!");
        // Do something with the message here
        return Task.CompletedTask;
    }
}