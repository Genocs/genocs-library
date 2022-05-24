using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.ServiceBusAzure.Service.Consumers;
using Genocs.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Genocs.Core.Demo.ServiceBusAzure.Service.Handlers;

public class DemoCommandHandler : ICommandHandler<DemoCommand>
{
    private readonly ILogger<SubmitOrderConsumer> _logger;

    public DemoCommandHandler(ILogger<SubmitOrderConsumer> logger)
    {
        _logger = logger;
    }

    public Task HandleCommand(DemoCommand command)
    {
        _logger.LogInformation(command.Payload);
        // Do something with the message here
        return Task.CompletedTask;
    }
}