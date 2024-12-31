using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Queries;
using Genocs.Orders.WebApi.Commands;
using Genocs.Orders.WebApi.DTO;
using Genocs.Orders.WebApi.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Orders.WebApi.Controllers;

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