using Genocs.Core.CQRS.Events;

namespace Genocs.Identities.Application.Events;

public class UserLocked : IEvent
{
    public Guid UserId { get; }

    public UserLocked(Guid userId)
    {
        UserId = userId;
    }
}