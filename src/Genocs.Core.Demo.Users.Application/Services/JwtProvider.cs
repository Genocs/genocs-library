using Convey.Auth;
using Genocs.Core.Demo.Users.Application.DTO;

namespace Genocs.Core.Demo.Users.Application.Services;

public class JwtProvider : IJwtProvider
{
    private readonly IJwtHandler _jwtHandler;

    public JwtProvider(IJwtHandler jwtHandler)
    {
        _jwtHandler = jwtHandler;
    }

    public AuthDto Create(Guid userId, string username, string role, string audience = null,
        IDictionary<string, IEnumerable<string>> claims = null)
    {
        var jwt = _jwtHandler.CreateToken(userId.ToString("N"), role, audience, claims);

        return new AuthDto
        {
            UserId = userId,
            Username = username,
            AccessToken = jwt.AccessToken,
            Role = jwt.Role,
            Expires = jwt.Expires
        };
    }
}