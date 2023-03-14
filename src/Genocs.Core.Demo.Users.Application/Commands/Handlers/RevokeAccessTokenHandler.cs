using Convey.Auth;
using Convey.CQRS.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace Trill.Services.Users.Core.Commands.Handlers
{
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
}