using Genocs.Identities.Application.Domain.Exceptions;

namespace Genocs.Identities.Application.Domain.Entities;

public class User : AggregateRoot
{
    public string Email { get; private set; }
    public string Name { get; private set; }
    public IEnumerable<string> Roles { get; private set; }
    public string Password { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IEnumerable<string>? Permissions { get; private set; }
    public bool Locked { get; private set; }

    public User(
                in Guid id,
                string email,
                string name,
                string password,
                IEnumerable<string> roles,
                in DateTime createdAt,
                IEnumerable<string>? permissions = null,
                bool locked = false)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidEmailException(email);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException(name);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidPasswordException();
        }

        if (!Role.IsValid(roles))
        {
            throw new InvalidRoleException(string.Join(",", roles));
        }

        Id = id;
        Email = email.ToLowerInvariant();
        Name = name.Trim();
        Password = password;
        Roles = roles.Select(c => c.ToLowerInvariant());
        CreatedAt = createdAt;
        Permissions = permissions ?? Enumerable.Empty<string>();
        Locked = locked;
    }

    public bool Lock()
    {
        if (Locked)
        {
            return false;
        }

        Locked = true;
        return true;
    }

    public bool Unlock()
    {
        if (!Locked)
        {
            return false;
        }

        Locked = false;
        return true;
    }
}