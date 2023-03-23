namespace Genocs.Identities.Application.Domain.Exceptions;

public class RevokedRefreshTokenException : DomainException
{
    public RevokedRefreshTokenException() : base("Revoked refresh token.")
    {
    }
}