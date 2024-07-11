using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Genocs.Core.Demo.WebApi.Infrastructure;

/// <summary>
/// Middleware authentication. Used to implement OpenId JWT implementation.
/// </summary>
public class FirebaseAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public FirebaseAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        string authHeader = context.Request.Headers["Authorization"].ToString();

        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            string token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                var jwtToken = new JwtSecurityToken(token);
                var payload = jwtToken.Payload;
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, payload["user_id"].ToString()),
                    new Claim(ClaimTypes.Name, payload["name"].ToString()),

                    // Add more claims as needed
                };

                var identity = new ClaimsIdentity(claims, "Firebase");
                context.User = new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
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