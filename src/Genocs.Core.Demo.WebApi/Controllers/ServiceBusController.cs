using Genocs.Core.Demo.Contracts;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ServiceBusController : ControllerBase
{
    private readonly IAzureServiceBusQueue _azureServiceBusQueue;
    private readonly IAzureServiceBusTopic _azureServiceBusTopic;

    private readonly ILogger<ServiceBusController> _logger;

    public ServiceBusController(ILogger<ServiceBusController> logger, IAzureServiceBusQueue azureServiceBusQueue, IAzureServiceBusTopic azureServiceBusTopic)
    {
        _logger = logger;
        _azureServiceBusQueue = azureServiceBusQueue;
        _azureServiceBusTopic = azureServiceBusTopic;
    }

    [HttpPost("SendToQueueAzureServiceBusQueue")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]

    public async Task<IActionResult> PostToQueueAzureServiceBus()
    {
        // Send command with AzureServiceBus
        await _azureServiceBusQueue.SendAsync(new DemoCommand("Test command"));

        return Ok("Sent");
    }

    [HttpPost("SendToTopicAzureServiceBusTopic")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostToTopicAzureServiceBus()
    {
        // Publish an event with AzureServiceBus
        Dictionary<string, object> filters = new Dictionary<string, object>();
        filters.Add("demotype", "demo1");

        await _azureServiceBusTopic.PublishAsync(new DemoEvent("demo event 1 subscription", "address"), filters);

        return Ok("Sent");
    }

    [HttpPost("SubmitOrder")]
    public async Task<IActionResult> PostSubmitOrder()
    {
        Dictionary<string, object> filters = new()
        {
            { "service", "OrderRequest" }
        };

        var azureServiceBusMessage = new OrderRequest
        {
            OrderId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            CardToken = "6500-1254-2548",
            Amount = 10.0M,
            Currency = "EUR",
            Basket = new List<Product>()
        };

        await _azureServiceBusTopic.PublishAsync(azureServiceBusMessage, filters);

        return Ok("Sent");
    }
}
