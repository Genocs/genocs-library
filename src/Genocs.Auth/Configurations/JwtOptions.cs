using Microsoft.IdentityModel.Tokens;

namespace Genocs.Auth.Configurations;

/// <summary>
/// The options class for configuring JWT (JSON Web Token) authentication in the Genocs authentication library.
/// </summary>
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

    /// <summary>
    /// Gets or sets the endpoints that allow anonymous access.
    /// </summary>
    public IEnumerable<string>? AllowAnonymousEndpoints { get; set; }

    /// <summary>
    /// Gets or sets the certificate options for JWT authentication.
    /// </summary>
    public CertificateOptions? Certificate { get; set; }

    /// <summary>
    /// The algorithm used to sign the token.
    /// Defaults to SecurityAlgorithms.HmacSha256 'HS256'.
    /// </summary>
    public string Algorithm { get; set; } = SecurityAlgorithms.HmacSha256;

    /// <summary>
    /// The issuer of the token. This is the entity that issues the token and is responsible for its validity.
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// Gets the security key used to validate the issuer's signature in token authentication.
    /// </summary>
    /// <remarks>The key should be provided in a format compatible with the authentication mechanism in use,
    /// such as a symmetric or asymmetric key. Ensure that the key is kept secure and not exposed in client
    /// applications.</remarks>
    public string IssuerSigningKey { get; set; }

    /// <summary>
    /// Gets the authority component used for authentication or identification purposes.
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// Gets the intended recipient or audience for the token.
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    /// The challenge string used in the Authenticate header.
    /// </summary>
    public string Challenge { get; set; } = "Bearer";

    /// <summary>
    /// The metadata address used to retrieve the OpenID Connect configuration from the authority.
    /// </summary>
    public string MetadataAddress { get; set; } = "/.well-known/openid-configuration";

    /// <summary>
    /// It defines whether the token should be saved in the authentication properties after a successful authentication.
    /// </summary>
    public bool SaveToken { get; set; } = true;
    public bool SaveSigninToken { get; set; }
    public bool RequireAudience { get; set; } = true;
    public bool RequireHttpsMetadata { get; set; }
    public bool RequireExpirationTime { get; set; } = true;
    public bool RequireSignedTokens { get; set; } = true;

    /// <summary>
    /// The expiration time of the token in minutes.
    /// Defaults to 60 minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;
    public TimeSpan? Expiry { get; set; }
    public string ValidAudience { get; set; }
    public IEnumerable<string>? ValidAudiences { get; set; }
    public string ValidIssuer { get; set; }
    public IEnumerable<string>? ValidIssuers { get; set; }
    public bool ValidateActor { get; set; }

    /// <summary>
    /// It defines whether the audience should be validated.
    /// Defaults to true.
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// It defines whether the issuer should be validated.
    /// Defaults to true.
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateTokenReplay { get; set; }
    public bool ValidateIssuerSigningKey { get; set; }

    /// <summary>
    /// It defines whether the token should be refreshed when the issuer key is not found.
    /// Defaults to true.
    /// </summary>
    public bool RefreshOnIssuerKeyNotFound { get; set; } = true;

    /// <summary>
    /// It defines whether the error details should be included in the response.
    /// Defaults to true.
    /// </summary>
    public bool IncludeErrorDetails { get; set; } = true;

    public string AuthenticationType { get; set; }
    public string NameClaimType { get; set; }

    /// <summary>
    /// The claim type that will be used to determine the user's roles.
    /// Default is "Role".
    /// </summary>
    public string RoleClaimType { get; set; } = "Role";

    public class CertificateOptions
    {
        /// <summary>
        /// The location of the certificate.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The certificate as a byte array.
        /// </summary>
        public string RawData { get; set; }

        /// <summary>
        /// The certificate password.
        /// </summary>
        public string Password { get; set; }
    }
}