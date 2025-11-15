namespace Genocs.Library.Demo.WebApi.Configurations;

/// <summary>
/// Configuration options for Firebase authorization.
/// </summary>
public class FirebaseAuthorizationOptions
{
    public const string Position = "FirebaseAuthorization";

    /// <summary>
    /// Default roles to assign to authenticated users.
    /// </summary>
    public List<string> DefaultRoles { get; set; } = ["User"];

    /// <summary>
    /// Email domain to role mappings.
    /// </summary>
    public Dictionary<string, List<string>> EmailDomainRoles { get; set; } = [];

    /// <summary>
    /// Specific email to role mappings.
    /// </summary>
    public Dictionary<string, List<string>> EmailRoles { get; set; } = [];

    /// <summary>
    /// Whether to require email verification.
    /// </summary>
    public bool RequireEmailVerification { get; set; } = true;

    /// <summary>
    /// Whether to enable demo mode (assigns Editor role to all users).
    /// </summary>
    public bool DemoMode { get; set; }
}