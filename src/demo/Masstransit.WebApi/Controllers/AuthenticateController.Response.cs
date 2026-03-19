namespace Genocs.Library.Demo.Masstransit.WebApi.Controllers;

/// <summary>
/// Response model for authentication validation.
/// </summary>
/// <remarks>
/// Contains authentication status information and user details.
/// Includes authentication type to distinguish between API key and JWT authentication.
/// Provides comprehensive claim information for debugging and integration testing.
/// </remarks>
public class AuthenticationResponse
{
    /// <summary>
    /// Indicates whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// The type of authentication used (apikey, firebase, etc.).
    /// </summary>
    public string? AuthenticationType { get; set; }

    /// <summary>
    /// The authenticated user's unique identifier.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The authenticated user's display name.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// List of all claims associated with the authenticated user.
    /// </summary>
    public List<ClaimInfo> Claims { get; set; } = [];
}

/// <summary>
/// Response model for user information.
/// </summary>
/// <remarks>
/// Contains basic user information extracted from JWT token.
/// Used for standard authenticated user operations.
/// </remarks>
public class UserInfoResponse
{
    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The user's display name.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The authentication method used.
    /// </summary>
    public string? AuthenticationType { get; set; }

    /// <summary>
    /// List of all claims associated with the authenticated user.
    /// </summary>
    public List<ClaimInfo> Claims { get; set; } = [];
}

/// <summary>
/// Represents a security claim with type and value.
/// </summary>
/// <remarks>
/// Used for debugging and integration testing to examine all available claims.
/// Provides insight into the authentication context and available user information.
/// </remarks>
public class ClaimInfo
{
    /// <summary>
    /// The claim type identifier.
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// The claim value.
    /// </summary>
    public string Value { get; set; } = default!;
}