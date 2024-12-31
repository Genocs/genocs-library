namespace Genocs.Identities.Application.Domain.Exceptions;

public class InvalidPasswordException : DomainException
{
    public InvalidPasswordException() : base($"Invalid password.")
    {
    }
}