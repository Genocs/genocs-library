namespace Genocs.Core.Demo.Users.Application.Domain.Exceptions;

public class InvalidAggregateIdException : DomainException
{
    public InvalidAggregateIdException() : base($"Invalid aggregate id.")
    {
    }
}