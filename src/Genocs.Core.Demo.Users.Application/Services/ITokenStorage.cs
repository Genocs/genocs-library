using Genocs.Core.Demo.Users.Application.DTO;

namespace Genocs.Core.Demo.Users.Application.Services;

public interface ITokenStorage
{
    void Set(Guid commandId, AuthDto token);
    AuthDto Get(Guid commandId);
}