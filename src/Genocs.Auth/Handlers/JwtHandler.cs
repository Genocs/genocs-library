using Genocs.Auth.Configurations;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Genocs.Auth.Handlers;

internal sealed class JwtHandler : IJwtHandler
{
    private static readonly IDictionary<string, IEnumerable<string>> EmptyClaims =
        new Dictionary<string, IEnumerable<string>>();

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
    private readonly string? _issuer;

    public JwtHandler(JwtOptions options, TokenValidationParameters tokenValidationParameters)
    {
        var issuerSigningKey = tokenValidationParameters.IssuerSigningKey;
        if (issuerSigningKey is null)
        {
            throw new InvalidOperationException("Issuer signing key not set.");
        }

        if (string.IsNullOrWhiteSpace(options.Algorithm))
        {
            throw new InvalidOperationException("Security algorithm not set.");
        }

        _options = options;
        _tokenValidationParameters = tokenValidationParameters;
        _signingCredentials = new SigningCredentials(issuerSigningKey, _options.Algorithm);
        _issuer = options.Issuer;
    }

    /// <summary>
    /// Creates a new token.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="role"></param>
    /// <param name="audience"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">It is thrown when mandatory data is empty.</exception>
    public JsonWebToken CreateToken(
                                    string userId,
                                    string? role = null,
                                    string? audience = null,
                                    IDictionary<string, IEnumerable<string>>? claims = null)
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

        if (!string.IsNullOrWhiteSpace(role))
        {
            jwtClaims.Add(new Claim(ClaimTypes.Role, role));
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
            _issuer,
            claims: jwtClaims,
            notBefore: now,
            expires: expires,
            signingCredentials: _signingCredentials);

        string token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new JsonWebToken
        {
            AccessToken = token,
            RefreshToken = string.Empty,
            Expires = expires.ToTimestamp(),
            Id = userId,
            Role = role ?? string.Empty,
            Claims = claims ?? EmptyClaims
        };
    }

    /// <summary>
    /// Gets the token payload.
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public JsonWebTokenPayload? GetTokenPayload(string accessToken)
    {
        _jwtSecurityTokenHandler.ValidateToken(
                                               accessToken,
                                               _tokenValidationParameters,
                                               out var validatedSecurityToken);

        if (validatedSecurityToken is not JwtSecurityToken jwt)
        {
            return null;
        }

        return new JsonWebTokenPayload
        {
            Subject = jwt.Subject,
            Role = jwt.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
            Expires = jwt.ValidTo.ToTimestamp(),
            Claims = jwt.Claims.Where(x => !DefaultClaims.Contains(x.Type))
                .GroupBy(c => c.Type)
                .ToDictionary(k => k.Key, v => v.Select(c => c.Value))
        };
    }
}