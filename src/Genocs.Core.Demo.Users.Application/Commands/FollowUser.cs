using Convey.CQRS.Commands;
using System;

namespace Trill.Services.Users.Core.Commands
{
    public class FollowUser : ICommand
    {
        public Guid UserId { get; }
        public Guid FolloweeId { get; }

        public FollowUser(Guid userId, Guid followeeId)
        {
            UserId = userId;
            FolloweeId = followeeId;
        }
    }
}