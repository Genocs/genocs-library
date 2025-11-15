using Genocs.Core.CQRS.Events;

namespace Genocs.Identities.Application.Events;

public class SignedIn : IEvent
{
    public Guid UserId { get; }

    public SignedIn(Guid userId)
    {
        UserId = userId;
    }
}