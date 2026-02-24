using Genocs.Common.Cqrs.Commands;

namespace Genocs.Identities.Application.Commands;

public class RevokeAccessToken(string accessToken) : ICommand
{
    public string AccessToken { get; } = accessToken;
}