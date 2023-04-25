using Genocs.Core.CQRS.Events;

namespace Genocs.Identities.Application.Events;

public class UserCreated : IEvent
{
    public Guid UserId { get; }
    public string Name { get; }
    public string Role { get; }

    public UserCreated(Guid userId, string name, string role)
    {
        UserId = userId;
        Name = name;
        Role = role;
    }
}