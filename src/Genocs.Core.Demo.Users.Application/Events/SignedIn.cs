using Convey.CQRS.Events;

namespace Genocs.Core.Demo.Users.Application.Events;

public class SignedIn : IEvent
{
    public Guid UserId { get; }

    public SignedIn(Guid userId)
    {
        UserId = userId;
    }
}