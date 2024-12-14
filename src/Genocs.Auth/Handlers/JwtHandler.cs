using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Genocs.Auth.Configurations;
using Microsoft.IdentityModel.Tokens;

namespace Genocs.Auth.Handlers;

internal sealed class JwtHandler : IJwtHandler
{
    private static readonly ISet<string> DefaultClaims = new HashSet<string>
    {
        JwtRegisteredClaimNames.Sub,
        JwtRegisteredClaimNames.UniqueName,
        JwtRegisteredClaimNames.Jti,
        JwtRegisteredClaimNames.Iat,
        ClaimTypes.Role,
    };

    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly SigningCredentials _signingCredentials;

    public JwtHandler(JwtOptions options, TokenValidationParameters tokenValidationParameters)
    {
        var issuerSigningKey = tokenValidationParameters.IssuerSigningKey
            ?? throw new InvalidOperationException("Issuer signing key not set.");

        if (string.IsNullOrWhiteSpace(options.Algorithm))
        {
            options.Algorithm = issuerSigningKey is SymmetricSecurityKey
                ? SecurityAlgorithms.HmacSha256
                : SecurityAlgorithms.RsaSha256;
        }

        _options = options;
        _tokenValidationParameters = tokenValidationParameters;
        _signingCredentials = new SigningCredentials(issuerSigningKey, _options.Algorithm);
    }

    /// <summary>
    /// Creates a new token.
    /// </summary>
    /// <param name="userId">The UserId.</param>
    /// <param name="roles">The User Role.</param>
    /// <param name="audience">The audience.</param>
    /// <param name="claims">The list of claims.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">It is thrown when mandatory data is empty.</exception>
    public JsonWebToken CreateToken(string userId, IEnumerable<string>? roles = null, string? audience = null, IDictionary<string, IEnumerable<string>>? claims = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID claim (subject) cannot be empty.", nameof(userId));
        }

        var now = DateTime.UtcNow;
        var jwtClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, now.ToTimestamp().ToString()),
        };

        if (roles is not null)
        {
            foreach (string item in roles.Where(x => !string.IsNullOrEmpty(x)))
            {
                jwtClaims.Add(new Claim(ClaimTypes.Role, item));
            }
        }

        if (!string.IsNullOrWhiteSpace(audience))
        {
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
        }

        if (claims?.Any() is true)
        {
            var customClaims = new List<Claim>();
            foreach (var (claim, values) in claims)
            {
                customClaims.AddRange(values.Select(value => new Claim(claim, value)));
            }

            jwtClaims.AddRange(customClaims);
        }

        var expires = _options.Expiry.HasValue
            ? now.AddMilliseconds(_options.Expiry.Value.TotalMilliseconds)
            : now.AddMinutes(_options.ExpiryMinutes);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            claims: jwtClaims,
            notBefore: now,
            expires: expires,
            signingCredentials: _signingCredentials);

        string token = _jwtSecurityTokenHandler.WriteToken(jwt);

        return new JsonWebToken
        {
            Id = userId,
            AccessToken = token,
            RefreshToken = string.Empty,
            Expires = expires.ToTimestamp(),
            Roles = roles,
            Claims = claims
        };
    }

    /// <summary>
    /// Gets the token payload.
    /// </summary>
    /// <param name="token">The string describing the access token.</param>
    /// <returns>The JWT Token payload.</returns>
    public JsonWebTokenPayload? GetTokenPayload(string token)
    {
        _jwtSecurityTokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedSecurityToken);

        if (validatedSecurityToken is not JwtSecurityToken jwt)
        {
            return null;
        }

        return new JsonWebTokenPayload
        {
            Subject = jwt.Subject,
            Roles = jwt.Claims.Where(x => x.Type == ClaimTypes.Role)?.Select(c => c.Value),
            Expires = jwt.ValidTo.ToTimestamp(),
            Claims = jwt.Claims.Where(x => !DefaultClaims.Contains(x.Type))
                .GroupBy(c => c.Type)
                .ToDictionary(k => k.Key, v => v.Select(c => c.Value))
        };
    }

    /// <summary>
    /// This method creates a token using the JwtSecurityTokenHandler class.
    /// </summary>
    /// <returns>The JWT token string.</returns>
    public string CreateToken()
    {
        SecurityTokenDescriptor tokenDescriptor = CreateSecurityTokenDescriptor();

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// This method creates a token using the JsonWebTokenHandler class.
    /// </summary>
    /// <returns>The JWT token string.</returns>
    public string CreateTokenWithJsonWebTokenHandler()
    {
        SecurityTokenDescriptor tokenDescriptor = CreateSecurityTokenDescriptor();

        var tokenHandler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        string token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    /// <summary>
    /// Internal method to create a SecurityTokenDescriptor.
    /// </summary>
    /// <returns>The created SecurityTokenDescriptor.</returns>
    private SecurityTokenDescriptor CreateSecurityTokenDescriptor()
    {
        byte[] key = Encoding.ASCII.GetBytes(_options.IssuerSigningKey!);
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.Role, "role")
            }),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), _options.Algorithm)
        };
    }
}