using Genocs.Core.CQRS.Events;

namespace Genocs.Template.Application.Events;

public class UserUnlocked : IEvent
{
    public Guid UserId { get; }

    public UserUnlocked(Guid userId)
    {
        UserId = userId;
    }
}