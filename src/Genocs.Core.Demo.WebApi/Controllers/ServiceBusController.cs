using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.WebApi.Models;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Genocs.Core.Demo.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceBusController : ControllerBase
    {
        private readonly IAzureServiceBusQueue _azureServiceBusQueue;
        private readonly IAzureServiceBusTopic _azureServiceBusTopic;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly ILogger<ServiceBusController> _logger;

        public ServiceBusController(ILogger<ServiceBusController> logger, IPublishEndpoint publishEndpoint,
                    IAzureServiceBusQueue azureServiceBusQueue, IAzureServiceBusTopic azureServiceBusTopic)
        {
            _logger = logger;
            _azureServiceBusQueue = azureServiceBusQueue;
            _azureServiceBusTopic = azureServiceBusTopic;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost("SubmitOrderToMasstransitBus")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostSubmitOrderToMasstransitBus()
        {
            // Publish an event with MassTransit
            await _publishEndpoint.Publish<SubmitOrder>(new
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString()
            });

            return Ok("Sent");
        }

        [HttpPost("SendToMasstransitBus")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostToMasstransitBus()
        {
            // Publish an event with MassTransit
            await _publishEndpoint.Publish<OrderSubmitted>(new
            {
                MerchantId = "0988656",
                OldStatus = "Approved",
                Status = "Rejected"
            });

            return Ok("Sent");
        }


        [HttpPost("SendToQueueAzureServiceBus")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]

        public async Task<IActionResult> PostToQueueAzureServiceBus()
        {
            // Send command with AzureServiceBus
            await _azureServiceBusQueue.SendAsync(new DemoCommand("Test command"));

            return Ok("Sent");
        }

        [HttpPost("SendToTopicAzureServiceBus")]
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

            await _publishEndpoint.Publish<SubmitOrder>(new
            {
                OrderId = Guid.NewGuid().ToString(),
                UserId = "userId"
            });

            //await _publishEndpoint.Publish<OrderTransactionSubmittedEvent>(new
            //{
            //    OrderId = Guid.NewGuid().ToString(),
            //    CorrelationId = guid,
            //    Credit = 100m,
            //    CustomerId = 123456
            //});

            //List<Product> prod = new List<Product>();
            //prod.Add(new Product { count = 1, price = 100.11m });
            //prod.Add(new Product { count = 2, price = 50m});
            //prod.Add(new Product { count = 3, price = 1.11m });

            //Mutex mutex = new Mutex(Guid.NewGuid().ToString());
            //var d = new Dictionary<string, object>
            //{
            //    { "test" , "1233546" },
            //    { "test1" , 456 },
            //    { "basket" , prod }
            //};

            //mutex.Data = d;
            //mutex.Process = "dsfasfsd";

            //var r = _mutexRepository.InsertOne(mutex);

            //_mutexRepository.ReplaceOne(updateFilter, mutex);

            //// Send command with AzureServiceBus
            //await _azureServiceBusQueue.SendAsync(new DemoCommand("Test command"));

            //// Publish an event with AzureServiceBus
            //Dictionary<string, object> filters = new Dictionary<string, object>();
            //filters.Add("demotype", "demo1");

            //await _azureServiceBusTopic.PublishAsync(new DemoEvent("demo event 1 subscription", "address"), filters);

            //// Publish an event with MassTransit
            //await _publishEndpoint.Publish<MerchantStatusChangedEvent>(new
            //{
            //    MerchantId = "0988656",
            //    OldStatus = "Approved",
            //    Status = "Rejected"
            //});

            return Ok("Sent");
        }
    }
}
