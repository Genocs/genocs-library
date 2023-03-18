using Genocs.Core.Demo.Users.Application.DTO;

namespace Genocs.Core.Demo.Users.Application.Services;

public interface IJwtProvider
{
    AuthDto Create(Guid userId, string username, string role, string audience = null,
        IDictionary<string, IEnumerable<string>> claims = null);
}