using Genocs.Template.Application.DTO;

namespace Genocs.Template.Application.Services;

public interface ITokenStorage
{
    void Set(Guid commandId, AuthDto token);
    AuthDto Get(Guid commandId);
}