using Genocs.Common.CQRS.Events;
using Genocs.Common.Types;
using Newtonsoft.Json;

namespace Genocs.Notifications.WebApi.Messages.Events;

[Message(exchange: "orders")]
[method: JsonConstructor]
public class OrderCreated(Guid orderId) : IEvent
{
    public Guid OrderId { get; } = orderId;
}