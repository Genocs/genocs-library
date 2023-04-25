using Genocs.Core.CQRS.Commands;

namespace Genocs.Identities.Application.Commands;

public class UnlockUser : ICommand
{
    public Guid UserId { get; }

    public UnlockUser(Guid userId)
    {
        UserId = userId;
    }
}