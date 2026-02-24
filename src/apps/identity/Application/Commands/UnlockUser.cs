using Genocs.Common.Cqrs.Commands;

namespace Genocs.Identities.Application.Commands;

public class UnlockUser(Guid userId) : ICommand
{
    public Guid UserId { get; } = userId;
}