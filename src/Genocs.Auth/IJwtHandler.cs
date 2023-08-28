namespace Genocs.Auth;

/// <summary>
/// IJwtHandler interface definition
/// </summary>
public interface IJwtHandler
{
    /// <summary>
    /// It allows to create a new JsonWebToken
    /// </summary>
    /// <param name="userId">The userId</param>
    /// <param name="role">The role</param>
    /// <param name="audience">The audience</param>
    /// <param name="claims">The claims</param>
    /// <returns>The JsonWebToken just created</returns>
    JsonWebToken CreateToken(string userId,
                             string? role = null,
                             string? audience = null,
                             IDictionary<string, IEnumerable<string>>? claims = null);

    /// <summary>
    /// Get the JsonWebTokenPayload from the accessToken
    /// </summary>
    /// <param name="accessToken">The access token string value</param>
    /// <returns>The JsonWebTokenPayload</returns>
    JsonWebTokenPayload? GetTokenPayload(string accessToken);
}