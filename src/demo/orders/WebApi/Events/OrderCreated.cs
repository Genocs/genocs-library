using Genocs.Common.CQRS.Events;

namespace Genocs.Orders.WebApi.Events;

public class OrderCreated(Guid orderId) : IEvent
{
    public Guid OrderId { get; } = orderId;
}