using Microsoft.AspNetCore.Identity;

namespace Genocs.Identities.Application.Services;

public class PasswordService : IPasswordService
{
    private readonly IPasswordHasher<IPasswordService> _passwordHasher;

    public PasswordService(IPasswordHasher<IPasswordService> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public bool IsValid(string hash, string password)
        => _passwordHasher.VerifyHashedPassword(this, hash, password) != PasswordVerificationResult.Failed;

    public string Hash(string password)
        => _passwordHasher.HashPassword(this, password);
}