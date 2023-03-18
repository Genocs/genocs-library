using Convey.CQRS.Commands;
using Genocs.Core.Demo.Users.Application.Domain.Exceptions;
using Genocs.Core.Demo.Users.Application.Domain.Repositories;

namespace Genocs.Core.Demo.Users.Application.Commands.Handlers;

internal sealed class RevokeRefreshTokenHandler : ICommandHandler<RevokeRefreshToken>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RevokeRefreshTokenHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task HandleAsync(RevokeRefreshToken command, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetAsync(command.RefreshToken);
        if (token is null)
        {
            throw new InvalidRefreshTokenException();
        }

        token.Revoke(DateTime.UtcNow);
        await _refreshTokenRepository.UpdateAsync(token);
    }
}