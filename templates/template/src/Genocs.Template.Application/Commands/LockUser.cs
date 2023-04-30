using Genocs.Core.CQRS.Commands;

namespace Genocs.Template.Application.Commands;

public class LockUser : ICommand
{
    public Guid UserId { get; }

    public LockUser(Guid userId)
    {
        UserId = userId;
    }
}