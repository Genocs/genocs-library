using Genocs.Core.CQRS.Commands;

namespace Genocs.Identities.Application.Commands;

public class CreateAdmin(string email, string name, string password, IEnumerable<string> permissions) : ICommand
{
    public Guid UserId { get; } = Guid.NewGuid();
    public string Email { get; } = email;
    public string Name { get; } = name;
    public string Password { get; } = password;
    public IEnumerable<string> Permissions { get; } = permissions;
}