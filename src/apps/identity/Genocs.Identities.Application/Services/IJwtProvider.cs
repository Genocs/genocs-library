using Genocs.Identities.Application.DTO;

namespace Genocs.Identities.Application.Services;

public interface IJwtProvider
{
    AuthDto Create(Guid userId, string username, string role, string audience = null,
        IDictionary<string, IEnumerable<string>> claims = null);
}