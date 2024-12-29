using Genocs.Core.CQRS.Commands;

namespace Genocs.Identities.Application.Commands;

public class LockUser(Guid userId) : ICommand
{
    public Guid UserId { get; } = userId;
}