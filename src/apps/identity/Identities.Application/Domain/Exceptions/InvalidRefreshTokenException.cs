namespace Genocs.Identities.Application.Domain.Exceptions;

public class InvalidRefreshTokenException : DomainException
{
    public InvalidRefreshTokenException() : base("Invalid refresh token.")
    {
    }
}