using Genocs.Common.Cqrs.Events;
using Genocs.Common.Types;
using Newtonsoft.Json;

namespace Genocs.Notifications.WebApi.Messages.Events;

[Message("operations")]
public class OperationRejected : IEvent
{
    public DefaultIdType Id { get; }
    public DefaultIdType UserId { get; }
    public string Name { get; }
    public string Resource { get; }
    public string Code { get; }
    public string Message { get; }

    [JsonConstructor]
    public OperationRejected(
                            DefaultIdType id,
                            DefaultIdType userId,
                            string name,
                            string resource,
                            string code,
                            string message)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Resource = resource;
        Code = code;
        Message = message;
    }
}