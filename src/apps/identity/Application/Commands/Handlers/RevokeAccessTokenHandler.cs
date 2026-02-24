using Genocs.Auth;
using Genocs.Common.Cqrs.Commands;

namespace Genocs.Identities.Application.Commands.Handlers;

public sealed class RevokeAccessTokenHandler(IAccessTokenService accessTokenService)
    : ICommandHandler<RevokeAccessToken>
{
    private readonly IAccessTokenService _accessTokenService = accessTokenService
        ?? throw new ArgumentNullException(nameof(accessTokenService));

    public async Task HandleAsync(RevokeAccessToken command, CancellationToken cancellationToken = default)
        => await Task.Run(() => { _accessTokenService.Deactivate(command.AccessToken); }, cancellationToken);
}