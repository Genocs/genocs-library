using Genocs.Core.Domain.Entities;
using Genocs.Identities.Application.Domain.Entities;

namespace Genocs.Identities.Application.Mongo.Documents;

public class UserDocument : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> Roles { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string>? Permissions { get; set; }
    public bool Locked { get; set; }

    public UserDocument()
    {
    }

    public UserDocument(User user)
    {
        Id = user.Id;
        Email = user.Email;
        Name = user.Name;
        Roles = user.Roles;
        Password = user.Password;
        CreatedAt = user.CreatedAt;
        Permissions = user.Permissions;
        Locked = user.Locked;
    }

    public User ToEntity() => new(Id, Email, Name, Password, Roles, CreatedAt, Permissions, Locked);

    public bool IsTransient()
    {
        throw new NotImplementedException();
    }
}