using Genocs.Core.CQRS.Queries;
using Genocs.Orders.WebApi.Domain;
using Genocs.Orders.WebApi.DTO;
using Genocs.Persistence.MongoDb.Domain.Repositories;

namespace Genocs.Orders.WebApi.Queries.Handlers;

public class GetOrderHandler : IQueryHandler<GetOrder, OrderDto>
{
    private readonly IMongoDbBaseRepository<Order, Guid> _repository;

    public GetOrderHandler(IMongoDbBaseRepository<Order, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// GetOrder query handler.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OrderDto?> HandleAsync(GetOrder query, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetAsync(query.OrderId);

        return order is null
            ? null
            : new OrderDto { Id = order.Id, CustomerId = order.CustomerId, TotalAmount = order.TotalAmount };
    }
}