using Genocs.Core.CQRS.Commands;

namespace Genocs.Identities.Application.Commands;

public class RevokeAccessToken(string accessToken) : ICommand
{
    public string AccessToken { get; } = accessToken;
}