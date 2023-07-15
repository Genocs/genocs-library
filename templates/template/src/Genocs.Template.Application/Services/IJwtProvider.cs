using Genocs.Template.Application.DTO;

namespace Genocs.Template.Application.Services;

/// <summary>
/// JwtProvider interface definition
/// </summary>
public interface IJwtProvider
{
    AuthDto Create(Guid userId, string username, string role, string? audience = null,
        IDictionary<string, IEnumerable<string>>? claims = null);
}