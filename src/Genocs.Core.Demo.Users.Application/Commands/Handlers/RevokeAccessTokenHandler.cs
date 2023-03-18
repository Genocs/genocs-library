using Convey.Auth;
using Convey.CQRS.Commands;

namespace Genocs.Core.Demo.Users.Application.Commands.Handlers;

internal sealed class RevokeAccessTokenHandler : ICommandHandler<RevokeAccessToken>
{
    private readonly IAccessTokenService _accessTokenService;

    public RevokeAccessTokenHandler(IAccessTokenService accessTokenService)
    {
        _accessTokenService = accessTokenService;
    }

    public async Task HandleAsync(RevokeAccessToken command, CancellationToken cancellationToken = default)
    {
        await _accessTokenService.DeactivateAsync(command.AccessToken);
    }
}