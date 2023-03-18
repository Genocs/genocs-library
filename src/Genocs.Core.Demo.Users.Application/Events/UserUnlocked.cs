using Convey.CQRS.Events;

namespace Genocs.Core.Demo.Users.Application.Events
{
    public class UserUnlocked : IEvent
    {
        public Guid UserId { get; }

        public UserUnlocked(Guid userId)
        {
            UserId = userId;
        }
    }
}