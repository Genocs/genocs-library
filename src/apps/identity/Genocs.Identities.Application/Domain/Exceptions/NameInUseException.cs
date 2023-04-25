namespace Genocs.Identities.Application.Domain.Exceptions;

public class NameInUseException : DomainException
{
    public string Name { get; }

    public NameInUseException(string name) : base($"Name {name} is already in use.")
    {
        Name = name;
    }
}