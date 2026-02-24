using Genocs.Common.Cqrs.Commands;

namespace Genocs.Identities.Application.Commands;

public class UseRefreshToken(string refreshToken) : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public string RefreshToken { get; } = refreshToken;
}