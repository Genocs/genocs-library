using Microsoft.AspNetCore.Authorization;

namespace Genocs.Auth;

public class AuthAttribute : AuthorizeAttribute
{
    public AuthAttribute(string scheme, string policy = "") : base(policy)
    {
        AuthenticationSchemes = scheme;
    }
}