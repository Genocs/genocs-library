using Convey.CQRS.Commands;

namespace Genocs.Core.Demo.Users.Application.Commands;

public class UnlockUser : ICommand
{
    public Guid UserId { get; }

    public UnlockUser(Guid userId)
    {
        UserId = userId;
    }
}