using Convey.CQRS.Commands;
using System;

namespace Trill.Services.Users.Core.Commands
{
    public class UnlockUser : ICommand
    {
        public Guid UserId { get; }

        public UnlockUser(Guid userId)
        {
            UserId = userId;
        }
    }
}