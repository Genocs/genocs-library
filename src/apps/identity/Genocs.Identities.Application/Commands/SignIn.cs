using Genocs.Core.CQRS.Commands;

namespace Genocs.Identities.Application.Commands;

public class SignIn(string name, string password) : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public string Password { get; set; } = password;
}