namespace Genocs.Core.Demo.Users.Application.Domain.Exceptions;

public class RevokedRefreshTokenException : DomainException
{
    public RevokedRefreshTokenException() : base("Revoked refresh token.")
    {
    }
}