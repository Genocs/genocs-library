using Genocs.Auth;
using Genocs.Identities.Application.DTO;

namespace Genocs.Identities.Application.Services;

public class JwtProvider(IJwtHandler jwtHandler) : IJwtProvider
{
    private readonly IJwtHandler _jwtHandler = jwtHandler;

    public AuthDto Create(Guid userId, string username, IEnumerable<string> roles, string? audience = null, IDictionary<string, IEnumerable<string>>? claims = null)
    {
        var jwt = _jwtHandler.CreateToken(userId.ToString("N"), roles, audience, claims);

        return new AuthDto
        {
            UserId = userId,
            Username = username,
            AccessToken = jwt.AccessToken,
            Roles = jwt.Roles,
            Expires = jwt.Expires
        };
    }
}