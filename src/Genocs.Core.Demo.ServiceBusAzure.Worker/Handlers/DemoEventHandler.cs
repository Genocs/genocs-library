using Genocs.Core.Demo.Contracts;
using Genocs.Core.Interfaces;

namespace Genocs.Core.Demo.ServiceBusAzure.Worker.Handlers;

public class DemoEventHandler : IEventHandler<DemoEvent>
{
    private readonly ILogger<DemoEventHandler> _logger;

    public DemoEventHandler(ILogger<DemoEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEvent(DemoEvent command)
    {
        _logger.LogInformation($"DemoEvent '{command.Name}' processed!");
        // Do something with the message here
        return Task.CompletedTask;
    }
}