using Genocs.Common.CQRS.Commands;
using Genocs.Library.Demo.Contracts;

namespace Genocs.Library.Demo.Masstransit.Worker.Handlers;

public class DemoCommandHandler : ICommandHandler<DemoCommand>
{
    private readonly ILogger<DemoCommandHandler> _logger;

    public DemoCommandHandler(ILogger<DemoCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(DemoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"DemoCommand '{command.Payload}' processed!");

        // Do something with the message here
        return Task.CompletedTask;
    }
}