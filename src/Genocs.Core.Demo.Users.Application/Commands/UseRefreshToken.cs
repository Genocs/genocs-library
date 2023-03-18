using Convey.CQRS.Commands;

namespace Genocs.Core.Demo.Users.Application.Commands;

public class UseRefreshToken : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public string RefreshToken { get; }

    public UseRefreshToken(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}