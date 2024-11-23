namespace Genocs.Auth;

/// <summary>
/// The JSON Web Token definition.
/// </summary>
public class JsonWebToken
{
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the access token expiration.
    /// </summary>
    public long Expires { get; set; }

    /// <summary>
    /// Gets or sets the access token unique identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the access token role.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// The claims.
    /// </summary>
    public IDictionary<string, IEnumerable<string>>? Claims { get; set; }
}