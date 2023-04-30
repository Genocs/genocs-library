using Genocs.Core.CQRS.Commands;

namespace Genocs.Template.Application.Commands;

public class UnlockUser : ICommand
{
    public Guid UserId { get; }

    public UnlockUser(Guid userId)
    {
        UserId = userId;
    }
}