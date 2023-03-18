namespace Genocs.Core.Demo.Users.Application.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email) : base($"Invalid email: {email}.")
    {
    }
}