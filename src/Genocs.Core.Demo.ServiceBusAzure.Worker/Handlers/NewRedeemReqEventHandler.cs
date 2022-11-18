using Genocs.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Genocs.Core.Demo.ServiceBusAzure.Service.Handlers;

public class NewRedeemReqEventHandler : IEventHandler<NewRedeemReqEvent>
{
    private readonly ILogger<NewRedeemReqEventHandler> _logger;

    public NewRedeemReqEventHandler(ILogger<NewRedeemReqEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEvent(NewRedeemReqEvent @event)
    {
        _logger.LogInformation("{0}, {1}", @event.RequestId, @event.Currency);

        // Do something with the message here
        return Task.CompletedTask;
    }
}
public class NewRedeemReqEvent : IEvent
{
    public string RequestId { get; set; }
    public string UserId { get; set; }
    public DateTime TimeStamp { get; set; }
    public string CardToken { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}