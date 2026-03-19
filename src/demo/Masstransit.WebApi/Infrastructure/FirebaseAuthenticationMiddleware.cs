using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Genocs.Library.Demo.Masstransit.WebApi.Infrastructure;

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
                if (payload.TryGetValue("email_verified", out object? emailVerifiedObj) &&
                    emailVerifiedObj is bool emailVerified && !emailVerified)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Email not verified!");
                    return;
                }

                var claims = new[]
                {
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