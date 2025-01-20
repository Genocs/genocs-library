using Genocs.Common.Types;
using Genocs.Core.CQRS.Events;
using Newtonsoft.Json;

namespace Genocs.SignalR.WebApi.Messages.Events;

[Message("operations")]
public class OperationRejected : IEvent
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Name { get; }
    public string Resource { get; }
    public string Code { get; }
    public string Message { get; }

    [JsonConstructor]
    public OperationRejected(
                            Guid id,
                            Guid userId,
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