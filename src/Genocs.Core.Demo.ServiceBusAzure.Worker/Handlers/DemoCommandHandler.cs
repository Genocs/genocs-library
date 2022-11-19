using Genocs.Core.Demo.Contracts;
using Genocs.Core.Interfaces;

namespace Genocs.Core.Demo.ServiceBusAzure.Worker.Handlers;

public class DemoCommandHandler : ICommandHandler<DemoCommand>
{
    private readonly ILogger<DemoCommandHandler> _logger;

    public DemoCommandHandler(ILogger<DemoCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleCommand(DemoCommand command)
    {
        _logger.LogInformation($"DemoCommand '{command.Payload}' processed!");
        // Do something with the message here
        return Task.CompletedTask;
    }
}