using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Queries;
using Genocs.Products.WebApi.Commands;
using Genocs.Products.WebApi.DTO;
using Genocs.Products.WebApi.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Products.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public ProductsController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductDto>> Get([FromRoute] GetProduct query)
    {
        var product = await _queryDispatcher.QueryAsync(query);
        if (product is null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpPost]
    public async Task<ActionResult> Post(CreateProduct command)
    {
        await _commandDispatcher.SendAsync(command);
        return CreatedAtAction(nameof(Get), new { productId = command.ProductId }, null);
    }
}