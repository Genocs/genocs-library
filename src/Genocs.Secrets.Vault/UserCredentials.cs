namespace Genocs.Secrets.Vault;

public record UserCredentials
{
    public string? Username { get; }
    public string? Password { get; }
}