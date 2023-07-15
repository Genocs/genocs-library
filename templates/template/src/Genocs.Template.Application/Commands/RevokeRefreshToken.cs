using Genocs.Core.CQRS.Commands;

namespace Genocs.Template.Application.Commands;

public class RevokeRefreshToken : ICommand
{
    public string RefreshToken { get; }

    public RevokeRefreshToken(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}