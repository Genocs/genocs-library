namespace Genocs.Template.Application.Domain.Exceptions;

public class InvalidRefreshTokenException : DomainException
{
    public InvalidRefreshTokenException() : base("Invalid refresh token.")
    {
    }
}