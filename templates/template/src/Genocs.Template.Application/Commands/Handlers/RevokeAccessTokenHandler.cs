using Genocs.Auth;
using Genocs.Core.CQRS.Commands;

namespace Genocs.Template.Application.Commands.Handlers;

internal sealed class RevokeAccessTokenHandler : ICommandHandler<RevokeAccessToken>
{
    private readonly IAccessTokenService _accessTokenService;

    public RevokeAccessTokenHandler(IAccessTokenService accessTokenService)
    {
        _accessTokenService = accessTokenService ?? throw new ArgumentNullException(nameof(accessTokenService));
    }

    public async Task HandleAsync(RevokeAccessToken command, CancellationToken cancellationToken = default)
        => await _accessTokenService.DeactivateAsync(command.AccessToken);
}