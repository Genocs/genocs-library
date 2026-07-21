using Genocs.Identities.Application.DTO;

namespace Genocs.Identities.Application.Services;

public interface ITokenStorage
{
    void Set(Guid commandId, AuthDto token);
    AuthDto Get(Guid commandId);
}