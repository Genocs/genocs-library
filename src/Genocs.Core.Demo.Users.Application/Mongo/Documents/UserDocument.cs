using Convey.Types;
using Genocs.Core.Demo.Users.Application.Domain.Entities;

namespace Genocs.Core.Demo.Users.Application.Mongo.Documents;

public class UserDocument : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
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
        Role = user.Role;
        Password = user.Password;
        CreatedAt = user.CreatedAt;
        Permissions = user.Permissions;
        Locked = user.Locked;
    }

    public User ToEntity() => new User(Id, Email, Name, Password, Role, CreatedAt, Permissions, Locked);
}