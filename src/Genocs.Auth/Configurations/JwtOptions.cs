using Microsoft.IdentityModel.Tokens;

namespace Genocs.Auth.Configurations;

public class JwtOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "jwt";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public IEnumerable<string>? AllowAnonymousEndpoints { get; internal set; }
    public CertificateOptions? Certificate { get; internal set; }

    /// <summary>
    /// The algorithm used to sign the token.
    /// Defaults to SecurityAlgorithms.HmacSha256 'HS256'.
    /// </summary>
    public string Algorithm { get; internal set; } = SecurityAlgorithms.HmacSha256;

    /// <summary>
    /// The issuer of the token. This is the entity that issues the token and is responsible for its validity.
    /// </summary>
    public string? Issuer { get; internal set; }

    /// <summary>
    /// Gets the security key used to validate the issuer's signature in token authentication.
    /// </summary>
    /// <remarks>The key should be provided in a format compatible with the authentication mechanism in use,
    /// such as a symmetric or asymmetric key. Ensure that the key is kept secure and not exposed in client
    /// applications.</remarks>
    public string? IssuerSigningKey { get; internal set; }

    /// <summary>
    /// Gets the authority component used for authentication or identification purposes.
    /// </summary>
    public string? Authority { get; internal set; }

    /// <summary>
    /// Gets the intended recipient or audience for the token.
    /// </summary>
    public string? Audience { get; internal set; }

    /// <summary>
    /// This is the Authentication Scheme name.
    /// </summary>
    public string Challenge { get; internal set; } = "Bearer";
    public string MetadataAddress { get; internal set; } = "/.well-known/openid-configuration";
    public bool SaveToken { get; internal set; } = true;
    public bool SaveSigninToken { get; internal set; }
    public bool RequireAudience { get; internal set; } = true;
    public bool RequireHttpsMetadata { get; internal set; }
    public bool RequireExpirationTime { get; internal set; } = true;
    public bool RequireSignedTokens { get; internal set; } = true;

    /// <summary>
    /// The expiration time of the token in minutes.
    /// Defaults to 60 minutes.
    /// </summary>
    public int ExpiryMinutes { get; internal set; } = 60;
    public TimeSpan? Expiry { get; internal set; }
    public string? ValidAudience { get; internal set; }
    public IEnumerable<string>? ValidAudiences { get; internal set; }
    public string? ValidIssuer { get; internal set; }
    public IEnumerable<string>? ValidIssuers { get; internal set; }
    public bool ValidateActor { get; internal set; }

    /// <summary>
    /// It defines whether the audience should be validated.
    /// Defaults to true.
    /// </summary>
    public bool ValidateAudience { get; internal set; } = true;

    /// <summary>
    /// It defines whether the issuer should be validated.
    /// Defaults to true.
    /// </summary>
    public bool ValidateIssuer { get; internal set; } = true;
    public bool ValidateLifetime { get; internal set; } = true;
    public bool ValidateTokenReplay { get; internal set; }
    public bool ValidateIssuerSigningKey { get; internal set; }

    /// <summary>
    /// It defines whether the token should be refreshed when the issuer key is not found.
    /// Defaults to true.
    /// </summary>
    public bool RefreshOnIssuerKeyNotFound { get; internal set; } = true;

    /// <summary>
    /// It defines whether the error details should be included in the response.
    /// Defaults to true.
    /// </summary>
    public bool IncludeErrorDetails { get; internal set; } = true;

    public string? AuthenticationType { get; internal set; }
    public string? NameClaimType { get; internal set; }

    /// <summary>
    /// The claim type that will be used to determine the user's roles.
    /// Default is "Role".
    /// </summary>
    public string RoleClaimType { get; internal set; } = "Role";

    public class CertificateOptions
    {
        /// <summary>
        /// The location of the certificate.
        /// </summary>
        public string? Location { get; internal set; }

        /// <summary>
        /// The certificate as a byte array.
        /// </summary>
        public string? RawData { get; internal set; }

        /// <summary>
        /// The certificate password.
        /// </summary>
        public string? Password { get; internal set; }
    }
}