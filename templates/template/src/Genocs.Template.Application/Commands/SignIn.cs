using Genocs.Core.CQRS.Commands;

namespace Genocs.Template.Application.Commands;

public class SignIn : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Password { get; set; }

    public SignIn(string name, string password)
    {
        Name = name;
        Password = password;
    }
}