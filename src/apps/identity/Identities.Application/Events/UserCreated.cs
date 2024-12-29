using Genocs.Core.CQRS.Events;

namespace Genocs.Identities.Application.Events;

public class UserCreated(Guid userId, string name, IEnumerable<string> roles) : IEvent
{
    public Guid UserId { get; } = userId;
    public string Name { get; } = name;
    public IEnumerable<string> Roles { get; } = roles;
}