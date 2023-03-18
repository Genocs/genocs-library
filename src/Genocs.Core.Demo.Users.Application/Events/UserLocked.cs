using Convey.CQRS.Events;

namespace Genocs.Core.Demo.Users.Application.Events
{
    public class UserLocked : IEvent
    {
        public Guid UserId { get; }

        public UserLocked(Guid userId)
        {
            UserId = userId;
        }
    }
}