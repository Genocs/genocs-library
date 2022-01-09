using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.WebApi.Models;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genocs.Core.Demo.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParticularController : ControllerBase
    {
        private readonly IAzureServiceBusQueue _azureServiceBusQueue;
        private readonly IAzureServiceBusTopic _azureServiceBusTopic;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly ILogger<ParticularController> _logger;

        public ParticularController(ILogger<ParticularController> logger, IPublishEndpoint publishEndpoint,
                    IAzureServiceBusQueue azureServiceBusQueue, IAzureServiceBusTopic azureServiceBusTopic)
        {
            _logger = logger;
            _azureServiceBusQueue = azureServiceBusQueue;
            _azureServiceBusTopic = azureServiceBusTopic;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {

            // Send command with AzureServiceBus
            await _azureServiceBusQueue.SendAsync(new DemoCommand("Test command"));

            // Publish an event with AzureServiceBus
            Dictionary<string, object> filters = new Dictionary<string, object>();
            filters.Add("demotype", "demo1");

            await _azureServiceBusTopic.PublishAsync(new DemoEvent("demo event 1 subscription", "address"), filters);

            // Publish an event with MassTransit
            await _publishEndpoint.Publish<OrderSubmitted>(new
            {
                MerchantId = "0988656",
                OldStatus = "Approved",
                Status = "Rejected"
            });

            return Ok();
        }
    }
}
