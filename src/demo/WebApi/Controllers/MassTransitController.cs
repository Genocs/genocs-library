using System.Net.Mime;
using Genocs.Library.Demo.Contracts;
using Genocs.Library.Demo.WebApi.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MassTransitController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    private readonly ILogger<MassTransitController> _logger;

    public MassTransitController(ILogger<MassTransitController> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    /// <summary>
    /// The Get method is a simple API endpoint that returns the name of the controller.
    /// It is used to verify that the controller is working and can be accessed successfully.
    /// </summary>
    /// <returns>The name of the controller.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok(nameof(MassTransitController));

    /// <summary>
    /// This is a demo API to publish a command using MassTransit.
    /// It will create a SubmitOrder command with random data and publish it to the message broker.
    /// You can subscribe to this command in your consumers to process it.
    /// </summary>
    /// <returns>The created SubmitOrder command.</returns>
    [HttpPost("SubmitDemoCommand")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SubmitOrder), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostSubmitDemoCommand()
    {
        SubmitOrder order = new SubmitOrder(DefaultIdType.NewGuid(), DefaultIdType.NewGuid().ToString(), DefaultIdType.NewGuid().ToString());

        // Publish an event with MassTransit
        await _publishEndpoint.Publish(order);

        _logger.LogInformation("SubmitOrder Sent");

        return Ok(order);
    }

    /// <summary>
    /// This is a demo API to publish an event using MassTransit.
    /// </summary>
    /// <returns>The created OrderSubmitted event.</returns>
    [HttpPost("SubmitDemoEvent")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(OrderSubmitted), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostSubmitDemoEvent()
    {
        // Setup the event
        OrderSubmitted orderSubmitted = new OrderSubmitted(OrderId: DefaultIdType.NewGuid().ToString(), UserId: DefaultIdType.NewGuid().ToString());

        // Publish an event with MassTransit
        await _publishEndpoint.Publish(orderSubmitted);
        _logger.LogInformation("OrderSubmitted Sent");

        return Ok(orderSubmitted);
    }
}
