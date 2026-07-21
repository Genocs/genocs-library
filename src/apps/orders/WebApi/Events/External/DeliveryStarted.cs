using Genocs.Common.CQRS.Events;
using Genocs.Messaging;

namespace Genocs.Orders.WebApi.Events.External;

[Message("deliveries")]
public class DeliveryStarted(Guid deliveryId) : IEvent
{
    public Guid DeliveryId { get; } = deliveryId;
}