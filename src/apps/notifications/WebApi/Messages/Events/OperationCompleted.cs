using Genocs.Common.CQRS.Events;
using Genocs.Common.Types;
using Newtonsoft.Json;

namespace Genocs.Notifications.WebApi.Messages.Events;

[Message("operations")]
public class OperationCompleted : IEvent
{
    public DefaultIdType Id { get; }
    public DefaultIdType UserId { get; }
    public string Name { get; }
    public string Resource { get; }

    [JsonConstructor]
    public OperationCompleted(DefaultIdType id, DefaultIdType userId, string name, string resource)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Resource = resource;
    }
}