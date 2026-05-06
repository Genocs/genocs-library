using Genocs.Common.CQRS.Events;
using Genocs.Common.Types;
using Newtonsoft.Json;

namespace Genocs.Notifications.WebApi.Messages.Events;

[Message("operations")]
[method: JsonConstructor]
public class OperationRejected(DefaultIdType id, DefaultIdType userId, string name, string resource, string code, string message) : IEvent
{
    public DefaultIdType Id { get; } = id;
    public DefaultIdType UserId { get; } = userId;
    public string Name { get; } = name;
    public string Resource { get; } = resource;
    public string Code { get; } = code;
    public string Message { get; } = message;
}