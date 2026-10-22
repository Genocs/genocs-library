using Microsoft.AspNetCore.Authorization;

namespace Genocs.Auth.Attributes;

/// <summary>
/// The authorization Attribute.
/// </summary>
public class AuthAttribute : AuthorizeAttribute
{
    /// <summary>
    /// The AuthAttribute constructor.
    /// </summary>
    /// <param name="scheme">The authorization schema.</param>
    /// <param name="policy">The authorization policy.</param>
    public AuthAttribute(string scheme, string policy = "")
        : base(policy)
    {
        AuthenticationSchemes = scheme;
    }
}