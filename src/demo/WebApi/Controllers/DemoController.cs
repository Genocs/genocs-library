using Genocs.Core.Demo.Contracts;
using Genocs.Library.Demo.WebApi.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Genocs.Library.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    private readonly ILogger<DemoController> _logger;

    public DemoController(ILogger<DemoController> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    [HttpPost("SubmitDemoCommand")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostSubmitDemoCommand()
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

    [HttpPost("SubmitDemoEvent")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostSubmitDemoEvent()
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
}
