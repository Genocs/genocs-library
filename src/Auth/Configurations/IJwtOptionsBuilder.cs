namespace Genocs.Auth.Configurations;

/// <summary>
/// The IJwtOptionsBuilder interface defines a builder pattern for configuring
/// JWT options used in authentication.
/// It provides methods to set various properties related to JWT token validation,
/// such as issuer signing key, issuer, expiry time, and audience validation settings.
/// The Build method returns a configured JwtOptions instance based on the provided settings.
/// </summary>
public interface IJwtOptionsBuilder
{
    IJwtOptionsBuilder WithIssuerSigningKey(string issuerSigningKey);
    IJwtOptionsBuilder WithIssuer(string issuer);
    IJwtOptionsBuilder WithExpiryMinutes(int expiryMinutes);
    IJwtOptionsBuilder WithLifetimeValidation(bool validateLifetime);
    IJwtOptionsBuilder WithAudienceValidation(bool validateAudience);
    IJwtOptionsBuilder WithValidAudience(string validAudience);
    JwtOptions Build();
}