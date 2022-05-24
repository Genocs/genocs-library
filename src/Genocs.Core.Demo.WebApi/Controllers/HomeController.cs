using Genocs.Core.Demo.Contracts;
using Genocs.Core.Interfaces;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    // private readonly IAzureServiceBusQueue _azureServiceBusQueue;
    private readonly IAzureServiceBusTopic _azureServiceBusTopic;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<HomeController> _logger;

    //public HomeController(ILogger<HomeController> logger, IPublishEndpoint publishEndpoint, 
    //            IAzureServiceBusQueue azureServiceBusQueue, IAzureServiceBusTopic azureServiceBusTopic,
    //            IMutexRepository mutexRepository)
    //{
    //    _logger = logger;
    //    _azureServiceBusQueue = azureServiceBusQueue;
    //    _azureServiceBusTopic = azureServiceBusTopic;
    //    _publishEndpoint = publishEndpoint;
    //    _mutexRepository = mutexRepository;
    //}

    public HomeController(ILogger<HomeController> logger, IPublishEndpoint publishEndpoint, IAzureServiceBusTopic azureServiceBusTopic)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _azureServiceBusTopic = azureServiceBusTopic ?? throw new ArgumentNullException(nameof(azureServiceBusTopic));
    }

    [HttpGet]
    public ActionResult Get()
        => Ok("Genocs.Core.Demo.WebApi");

    [HttpGet("ping")]
    public ActionResult Ping()
        => Ok("pong");

    [HttpPost("SubmitOrder")]
    public async Task<ActionResult> PostSubmitOrder()
    {
        Dictionary<string, object> filters = new()
        {
            { "service", "LateRefund" }
        };

        var azureServiceBusMessage = new NewRedeemReqEvent
        {
            TimeStamp = DateTime.UtcNow,
            Amount = 10.0M,
            Currency = "EUR",
            UserId = Guid.NewGuid().ToString(),
            CardToken = "6500-1254-2548",
            RequestId = Guid.NewGuid().ToString()
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

        return Ok();
    }
}

public class Product
{
    public int Count { get; set; }
    public decimal Price { get; set; }
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
