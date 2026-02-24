using Genocs.Common.Cqrs.Events;
using Genocs.Messaging;

namespace Genocs.Orders.WebApi.Events.External;

[Message("deliveries")]
public class DeliveryStarted : IEvent
{
    public Guid DeliveryId { get; }

    public DeliveryStarted(Guid deliveryId)
    {
        DeliveryId = deliveryId;
    }
}