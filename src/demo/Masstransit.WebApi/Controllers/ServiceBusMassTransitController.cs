using System.Net.Mime;
using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Library.Demo.Contracts;
using Genocs.Library.Demo.Masstransit.WebApi.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.Masstransit.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ServiceBusMassTransitController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    private readonly ILogger<ServiceBusMassTransitController> _logger;

    public ServiceBusMassTransitController(ILogger<ServiceBusMassTransitController> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    /// <summary>
    /// The Get method is a simple endpoint that returns a string message indicating the name of the controller.
    /// It serves as a basic test to confirm that the controller is set up correctly and can respond to Http GET requests.
    /// </summary>
    /// <returns>A string message indicating the name of the controller.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public IActionResult Get()
    => Ok(nameof(ServiceBusMassTransitController));

    /// <summary>
    /// This endpoint is used to publish a SubmitOrder event to the message bus.
    /// The event contains a unique Id, OrderId, and UserId.
    /// The event is published using MassTransit, which will handle the serialization and sending of the message to the appropriate queue or topic.
    /// The endpoint returns a 200 OK response with a message indicating that the event was sent successfully.
    /// </summary>
    /// <returns>A 200 OK response with a message indicating that the event was sent successfully.</returns>
    [HttpPost("SubmitOrder")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostSubmitOrder()
    {
        // Publish an event with MassTransit
        await _publishEndpoint.Publish<SubmitOrder>(new
        {
            Id = DefaultIdType.NewGuid().ToString(),
            OrderId = DefaultIdType.NewGuid().ToString(),
            UserId = DefaultIdType.NewGuid().ToString()
        });

        _logger.LogInformation("SubmitOrder Sent");

        return Ok("Sent");
    }

    /// <summary>
    /// This endpoint is used to publish an OrderSubmitted event to the message bus.
    /// The event contains a MerchantId, OldStatus, and Status.
    /// The event is published using MassTransit, which will handle the serialization and sending of the message to the appropriate queue or topic.
    /// The endpoint returns a 200 OK response with a message indicating that the event was sent successfully.
    /// </summary>
    /// <returns>A 200 OK response with a message indicating that the event was sent successfully.</returns>
    [HttpPost("OrderSubmitted")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostOrderSubmitted()
    {
        // Publish an event with MassTransit
        await _publishEndpoint.Publish<OrderSubmitted>(new
        {
            MerchantId = "0988656",
            OldStatus = "Approved",
            Status = "Rejected"
        });

        _logger.LogInformation("OrderSubmitted Sent");

        return Ok("Sent");
    }

    //[HttpPost("SubmitOrder")]
    //public async Task<IActionResult> PostSubmitOrder()
    //{
    //    await _publishEndpoint.Publish<SubmitOrder>(new
    //    {
    //        OrderId = Guid.NewGuid().ToString(),
    //        UserId = "userId"
    //    });

    //    //await _publishEndpoint.Publish<OrderTransactionSubmittedEvent>(new
    //    //{
    //    //    OrderId = Guid.NewGuid().ToString(),
    //    //    CorrelationId = guid,
    //    //    Credit = 100m,
    //    //    CustomerId = 123456
    //    //});

    //    //List<Product> prod = new List<Product>();
    //    //prod.Add(new Product { count = 1, price = 100.11m });
    //    //prod.Add(new Product { count = 2, price = 50m});
    //    //prod.Add(new Product { count = 3, price = 1.11m });

    //    //Mutex mutex = new Mutex(Guid.NewGuid().ToString());
    //    //var d = new Dictionary<string, object>
    //    //{
    //    //    { "test" , "1233546" },
    //    //    { "test1" , 456 },
    //    //    { "basket" , prod }
    //    //};

    //    //mutex.Data = d;
    //    //mutex.Process = "dsfasfsd";

    //    //var r = _mutexRepository.InsertOne(mutex);

    //    //_mutexRepository.ReplaceOne(updateFilter, mutex);

    //    //// Send command with AzureServiceBus
    //    //await _azureServiceBusQueue.SendAsync(new DemoCommand("Test command"));

    //    //// Publish an event with AzureServiceBus
    //    //Dictionary<string, object> filters = new Dictionary<string, object>();
    //    //filters.Add("demotype", "demo1");

    //    //await _azureServiceBusTopic.PublishAsync(new DemoEvent("demo event 1 subscription", "address"), filters);

    //    //// Publish an event with MassTransit
    //    //await _publishEndpoint.Publish<MerchantStatusChangedEvent>(new
    //    //{
    //    //    MerchantId = "0988656",
    //    //    OldStatus = "Approved",
    //    //    Status = "Rejected"
    //    //});

    //    return Ok("Sent");
    //}
}
