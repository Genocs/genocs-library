using Genocs.Identities.Application.DTO;

namespace Genocs.Identities.Application.Services;

/// <summary>
/// JwtProvider interface definition.
/// </summary>
public interface IJwtProvider
{
    AuthDto Create(Guid userId, string username, IEnumerable<string> roles, string? audience = null, IDictionary<string, IEnumerable<string>>? claims = null);
}