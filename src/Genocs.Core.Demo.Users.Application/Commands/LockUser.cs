using Genocs.Core.CQRS.Commands;

namespace Genocs.Core.Demo.Users.Application.Commands;

public class LockUser : ICommand
{
    public Guid UserId { get; }

    public LockUser(Guid userId)
    {
        UserId = userId;
    }
}