using Convey.CQRS.Commands;

namespace Genocs.Core.Demo.Users.Application.Commands;

public class RevokeRefreshToken : ICommand
{
    public string RefreshToken { get; }

    public RevokeRefreshToken(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}