namespace Genocs.Secrets.HashicorpKeyVault;

public record UserCredentials
{
    public string? Username { get; }
    public string? Password { get; }
}