using Genocs.Auth.Options;

namespace Genocs.Auth;

public interface IJwtOptionsBuilder
{
    IJwtOptionsBuilder WithIssuerSigningKey(string issuerSigningKey);
    IJwtOptionsBuilder WithIssuer(string issuer);
    IJwtOptionsBuilder WithExpiryMinutes(int expiryMinutes);
    IJwtOptionsBuilder WithLifetimeValidation(bool validateLifetime);
    IJwtOptionsBuilder WithAudienceValidation(bool validateAudience);
    IJwtOptionsBuilder WithValidAudience(string validAudience);
    JwtSettings Build();
}