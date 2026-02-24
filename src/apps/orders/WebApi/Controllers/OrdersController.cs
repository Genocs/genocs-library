using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;
using Genocs.Orders.WebApi.Commands;
using Genocs.Orders.WebApi.DTO;
using Genocs.Orders.WebApi.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Orders.WebApi.Controllers;

/// <summary>
/// The OrdersController class is an API controller that handles HTTP requests related to orders.
/// It uses the CQRS pattern to separate command and query handling.
/// The controller provides endpoints for creating new orders and retrieving existing orders by their ID.
/// NOTE: This controller is meant to show how to implement hybrid Controller path: Minimal APIs vs. Controllers.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public OrdersController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> Get([FromRoute] GetOrder query)
    {
        var order = await _queryDispatcher.QueryAsync(query);
        return order is null ? (ActionResult<OrderDto>)NotFound() : (ActionResult<OrderDto>)order;
    }

    [HttpPost]
    public async Task<ActionResult> Post(CreateOrder command)
    {
        await _commandDispatcher.SendAsync(command);
        return CreatedAtAction(nameof(Get), new { orderId = command.OrderId }, null);
    }
}