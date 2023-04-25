using Microsoft.AspNetCore.Authorization;

namespace Genocs.Auth;

/// <summary>
/// The authorization Attribute
/// </summary>
public class AuthAttribute : AuthorizeAttribute
{
    /// <summary>
    /// The AuthAttribute constructor
    /// </summary>
    /// <param name="scheme"></param>
    /// <param name="policy"></param>
    public AuthAttribute(string scheme, string policy = "") : base(policy)
    {
        AuthenticationSchemes = scheme;
    }
}