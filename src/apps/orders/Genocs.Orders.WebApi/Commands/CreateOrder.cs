using Genocs.Core.CQRS.Commands;

namespace Genocs.Orders.WebApi.Commands;

public class CreateOrder : ICommand
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public Guid ProductId { get; }

    public CreateOrder(Guid orderId, Guid customerId, Guid productId)
    {
        OrderId = orderId == Guid.Empty ? Guid.NewGuid() : orderId;
        CustomerId = customerId;
        ProductId = productId;
    }
}