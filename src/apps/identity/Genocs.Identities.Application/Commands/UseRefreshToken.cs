using Genocs.Core.CQRS.Commands;

namespace Genocs.Identities.Application.Commands;

public class UseRefreshToken : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public string RefreshToken { get; }

    public UseRefreshToken(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}