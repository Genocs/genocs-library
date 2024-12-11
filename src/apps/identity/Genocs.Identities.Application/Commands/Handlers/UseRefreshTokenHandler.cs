using Genocs.Core.CQRS.Commands;
using Genocs.Identities.Application.Domain.Exceptions;
using Genocs.Identities.Application.Domain.Repositories;
using Genocs.Identities.Application.Exceptions;
using Genocs.Identities.Application.Services;

namespace Genocs.Identities.Application.Commands.Handlers;

internal sealed class UseRefreshTokenHandler : ICommandHandler<UseRefreshToken>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly ITokenStorage _storage;

    public UseRefreshTokenHandler(IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IJwtProvider jwtProvider, ITokenStorage storage)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _storage = storage;
    }

    public async Task HandleAsync(UseRefreshToken command, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetAsync(command.RefreshToken) ?? throw new InvalidRefreshTokenException();
        if (token.Revoked)
        {
            throw new RevokedRefreshTokenException();
        }

        var user = await _userRepository.GetAsync(token.UserId) ?? throw new UserNotFoundException(token.UserId);

        var claims = user.Permissions.Any()
            ? new Dictionary<string, IEnumerable<string>>
            {
                ["permissions"] = user.Permissions
            }
            : null;

        var auth = _jwtProvider.Create(token.UserId, user.Name, user.Roles, claims: claims);
        auth.RefreshToken = command.RefreshToken;
        _storage.Set(command.Id, auth);
    }
}