namespace Genocs.Identities.Application.Domain.Exceptions;

public class InvalidRoleException(string role)
    : DomainException($"Invalid role: {role}.")
{
}