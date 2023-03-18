namespace Genocs.Core.Demo.Users.Application.Domain.Exceptions;

public class InvalidNameException : DomainException
{
    public InvalidNameException(string name) : base($"Invalid name: {name}.")
    {
    }
}