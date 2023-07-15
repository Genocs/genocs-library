using Genocs.Core.CQRS.Commands;

namespace Genocs.Template.Application.Commands;

public class RevokeAccessToken : ICommand
{
    public string AccessToken { get; }

    public RevokeAccessToken(string accessToken)
    {
        AccessToken = accessToken;
    }
}