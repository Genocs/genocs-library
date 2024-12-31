using Genocs.Core.CQRS.Events;

namespace Genocs.Orders.WebApi.Events;

public class OrderCreated : IEvent
{
    public Guid OrderId { get; }

    public OrderCreated(Guid orderId)
    {
        OrderId = orderId;
    }
}