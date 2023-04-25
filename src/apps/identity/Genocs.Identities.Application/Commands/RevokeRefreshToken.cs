using Genocs.Core.CQRS.Commands;

namespace Genocs.Identities.Application.Commands;

public class RevokeRefreshToken : ICommand
{
    public string RefreshToken { get; }

    public RevokeRefreshToken(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}