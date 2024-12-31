namespace Genocs.Identities.Application.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email) : base($"Invalid email: {email}.")
    {
    }
}