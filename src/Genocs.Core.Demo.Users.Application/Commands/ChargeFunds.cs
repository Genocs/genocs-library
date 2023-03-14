using Convey.CQRS.Commands;
using System;

namespace Trill.Services.Users.Core.Commands
{
    public class ChargeFunds : ICommand
    {
        public Guid UserId { get; }
        public decimal Amount { get; }

        public ChargeFunds(Guid userId, decimal amount)
        {
            UserId = userId;
            Amount = amount;
        }
    }
}