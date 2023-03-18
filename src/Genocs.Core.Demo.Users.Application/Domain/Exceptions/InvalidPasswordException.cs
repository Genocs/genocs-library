namespace Genocs.Core.Demo.Users.Application.Domain.Exceptions;

public class InvalidPasswordException : DomainException
{
    public InvalidPasswordException() : base($"Invalid password.")
    {
    }
}