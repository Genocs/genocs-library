using Genocs.Core.CQRS.Queries;
using Genocs.Orders.WebApi.Domain;
using Genocs.Orders.WebApi.DTO;
using Genocs.Persistence.MongoDb.Legacy;

namespace Genocs.Orders.WebApi.Queries.Handlers;

public class GetOrderHandler : IQueryHandler<GetOrder, OrderDto>
{
    private readonly IMongoRepository<Order, Guid> _repository;

    public GetOrderHandler(IMongoRepository<Order, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto> HandleAsync(GetOrder query, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetAsync(query.OrderId);

        return order is null
            ? null
            : new OrderDto { Id = order.Id, CustomerId = order.CustomerId, TotalAmount = order.TotalAmount };
    }
}