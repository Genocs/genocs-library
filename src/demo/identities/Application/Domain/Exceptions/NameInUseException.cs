namespace Genocs.Identities.Application.Domain.Exceptions;

public class NameInUseException(string name) : DomainException($"Name {name} is already in use.")
{
    public string Name { get; } = name;
}