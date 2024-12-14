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

    public IEnumerable<string>? AllowAnonymousEndpoints { get; set; }
    public CertificateOptions? Certificate { get; set; }

    /// <summary>
    /// The algorithm used to sign the token.
    /// Defaults to SecurityAlgorithms.HmacSha256 'HS256'.
    /// </summary>
    public string Algorithm { get; set; } = SecurityAlgorithms.HmacSha256;
    public string? Issuer { get; set; }
    public string? IssuerSigningKey { get; set; }
    public string? Authority { get; set; }
    public string? Audience { get; set; }

    /// <summary>
    /// This is the Authentication Scheme name.
    /// </summary>
    public string Challenge { get; set; } = "Bearer";
    public string MetadataAddress { get; set; } = "/.well-known/openid-configuration";
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
    public string? ValidAudience { get; set; }
    public IEnumerable<string>? ValidAudiences { get; set; }
    public string? ValidIssuer { get; set; }
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

    public string? AuthenticationType { get; set; }
    public string? NameClaimType { get; set; }

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
        public string? Location { get; set; }

        /// <summary>
        /// The certificate as a byte array.
        /// </summary>
        public string? RawData { get; set; }

        /// <summary>
        /// The certificate password.
        /// </summary>
        public string? Password { get; set; }
    }
}