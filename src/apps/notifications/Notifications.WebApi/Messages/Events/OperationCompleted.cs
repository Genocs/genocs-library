using Genocs.Common.Types;
using Genocs.Core.CQRS.Events;
using Newtonsoft.Json;

namespace Genocs.SignalR.WebApi.Messages.Events;

[Message("operations")]
public class OperationCompleted : IEvent
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Name { get; }
    public string Resource { get; }

    [JsonConstructor]
    public OperationCompleted(
                                Guid id,
                                Guid userId,
                                string name,
                                string resource)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Resource = resource;
    }
}