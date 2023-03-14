using Convey.CQRS.Commands;
using System;

namespace Trill.Services.Users.Core.Commands
{
    public class LockUser : ICommand
    {
        public Guid UserId { get; }

        public LockUser(Guid userId)
        {
            UserId = userId;
        }
    }
}