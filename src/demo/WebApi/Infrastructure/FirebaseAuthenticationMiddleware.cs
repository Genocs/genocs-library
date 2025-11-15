using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Genocs.Library.Demo.WebApi.Infrastructure;

/// <summary>
/// Middleware authentication. Used to implement OpenId JWT implementation.
/// </summary>
public class FirebaseAuthenticationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        string authHeader = context.Request.Headers.Authorization.ToString();

        if (authHeader?.StartsWith("Bearer ") == true)
        {
            string token = authHeader["Bearer ".Length..].Trim();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                var jwtToken = new JwtSecurityToken(token);
                var payload = jwtToken.Payload;

                // Only account with email_verified = true are allowed
                if (payload.TryGetValue("email_verified", out var emailVerifiedObj) &&
                    emailVerifiedObj is bool emailVerified && !emailVerified)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Email not verified!");
                    return;
                }

                var claims = new[]
                {
                    // Available claims in the Firebase token payload:
                    // ["name"]    "Giovanni Emanuele Nocco"   System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["iss"] "https://securetoken.google.com/fiscanner-web"  System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["aud"] "fiscanner-web" System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["auth_time"]   1758541465  System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["user_id"] "7tlWHAOd8IfoqRwbiwEvD5OuzHJ2"  System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["sub"] "7tlWHAOd8IfoqRwbiwEvD5OuzHJ2"  System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["iat"] 1758541465  System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["exp"] 1758545065  System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["email"]   "giovanni.nocco@hotmail.com"    System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["email_verified"]  false   System.Collections.Generic.DebugViewDictionaryItem<string, object>
                    // ["firebase"]    ValueKind = Object : "{"identities":{"email":["giovanni.nocco @hotmail.com"]},"sign_in_provider":"password"}"    System.Collections.Generic.DebugViewDictionaryItem<string, object>

                    new Claim(ClaimTypes.NameIdentifier, payload["user_id"].ToString()),
                    new Claim(ClaimTypes.Name, payload["name"].ToString()),

                    // Add more claims as needed
                };

                var identity = new ClaimsIdentity(claims, "Firebase");
                context.User = new ClaimsPrincipal(identity);
            }
            catch (Exception)
            {
                // Token validation failed
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        await _next(context);
    }
}