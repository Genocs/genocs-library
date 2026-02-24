using Genocs.Common.Cqrs.Commands;

namespace Genocs.Identities.Application.Commands;

public class RevokeRefreshToken(string refreshToken) : ICommand
{
    public string RefreshToken { get; } = refreshToken;
}