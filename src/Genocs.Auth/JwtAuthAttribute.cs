using Genocs.Auth.Attributes;

namespace Genocs.Auth;

public class JwtAuthAttribute(string policy = "")
    : AuthAttribute(AuthenticationScheme, policy)
{
    public const string AuthenticationScheme = "Bearer";
}