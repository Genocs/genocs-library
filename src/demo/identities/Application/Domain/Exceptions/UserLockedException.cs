namespace Genocs.Identities.Application.Domain.Exceptions;

public class UserLockedException(ref readonly Guid userId) : DomainException($"User with ID: '{userId}' is locked.")
{
    public Guid UserId { get; } = userId;
}