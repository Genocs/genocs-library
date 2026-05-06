using Genocs.Common.CQRS.Events;
using Genocs.Common.Types;
using Newtonsoft.Json;

namespace Genocs.Notifications.WebApi.Messages.Events;

[Message("operations")]
[method: JsonConstructor]
public class OperationPending(DefaultIdType id, DefaultIdType userId, string name, string resource) : IEvent
{
    public DefaultIdType Id { get; } = id;
    public DefaultIdType UserId { get; } = userId;
    public string Name { get; } = name;
    public string Resource { get; } = resource;
}