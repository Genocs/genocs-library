namespace Genocs.Identities.Application.Domain.Exceptions;

public class EmptyRefreshTokenException : DomainException
{
    public EmptyRefreshTokenException() : base("Empty refresh token.")
    {
    }
}