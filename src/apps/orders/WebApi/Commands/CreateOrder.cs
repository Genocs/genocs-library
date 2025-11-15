using Genocs.Core.CQRS.Commands;

namespace Genocs.Orders.WebApi.Commands;

public class CreateOrder(Guid orderId, Guid customerId, List<Guid> products) : ICommand
{
    public Guid OrderId { get; } = orderId == Guid.Empty ? Guid.NewGuid() : orderId;
    public Guid CustomerId { get; } = customerId;
    public List<Guid> Products { get; } = products;
}