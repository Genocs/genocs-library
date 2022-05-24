using Genocs.Core.Demo.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Genocs.Core.Demo.ServiceBusAzure.Service.Consumers;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly ILogger<SubmitOrderConsumer> _logger;

    public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SubmitOrder> context)
    {
        throw new System.NotImplementedException();
    }
}