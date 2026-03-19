using Genocs.Common.CQRS.Queries;
using Genocs.Orders.WebApi.Domain;
using Genocs.Orders.WebApi.DTO;
using Genocs.Persistence.MongoDB.Domain.Repositories;

namespace Genocs.Orders.WebApi.Queries.Handlers;

public class GetOrderHandler : IQueryHandler<GetOrder, OrderDto>
{
    private readonly IMongoBaseRepository<Order, Guid> _repository;

    public GetOrderHandler(IMongoBaseRepository<Order, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// GetOrder query handler.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The order DTO.</returns>
    public async Task<OrderDto?> HandleAsync(GetOrder query, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetAsync(query.OrderId, cancellationToken);

        return order is null
            ? null
            : new OrderDto { Id = order.Id, CustomerId = order.CustomerId, TotalAmount = order.TotalAmount };
    }
}