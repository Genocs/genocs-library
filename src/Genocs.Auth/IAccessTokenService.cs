namespace Genocs.Auth;

public interface IAccessTokenService
{
    bool IsCurrentActiveToken();
    void DeactivateCurrent();
    bool IsActive(string token);
    void Deactivate(string token);
}