using System.Net.Mime;
using Genocs.Library.Demo.Contracts;
using Genocs.Library.Demo.Domain.Aggregates;
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
        await _publishEndpoint.Publish(new OrderSubmitted(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

        _logger.LogInformation("OrderSubmitted Sent");

        return Ok("Sent");
    }
}
