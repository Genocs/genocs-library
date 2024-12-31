using Genocs.Core.CQRS.Commands;

namespace Genocs.Identities.Application.Commands;

public class RevokeRefreshToken(string refreshToken) : ICommand
{
    public string RefreshToken { get; } = refreshToken;
}