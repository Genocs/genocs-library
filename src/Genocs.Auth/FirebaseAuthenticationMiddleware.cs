using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Genocs.Auth;

/// <summary>
/// Middleware for handling JWT authentication and API key authentication.
/// </summary>
/// <remarks>
/// This middleware supports dual authentication modes:
/// 1. API Key authentication via x-gnx-apikey header for system-to-system communication
/// 2. JWT Bearer token authentication for user authentication
/// When API key is provided, it bypasses JWT validation and sets up API key-based claims.
/// </remarks>
public class JWTOrApiKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;

    public async Task Invoke(HttpContext context)
    {
        // Check for API key authentication first
        string? apiKey = context.Request.Headers["x-gnx-apikey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            if (await ValidateApiKeyAsync(apiKey))
            {
                // Set up API key-based authentication
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "api-client"),
                    new Claim(ClaimTypes.Name, "API Client"),
                    new Claim("auth_type", "apikey"),
                    new Claim("api_key", apiKey)
                };

                var identity = new ClaimsIdentity(claims, "ApiKey");
                context.User = new ClaimsPrincipal(identity);

                await _next(context);
                return;
            }
            else
            {
                // Invalid API key
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }
        }

        // Check for Firebase JWT authentication
        string? authHeader = context.Request.Headers.Authorization;
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            string token = authHeader["Bearer ".Length..].Trim();

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken ?? throw new SecurityTokenException("Invalid token format");
                var payload = jsonToken.Payload;
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, payload["user_id"]?.ToString() ?? string.Empty),
                    new Claim(ClaimTypes.Name, payload["name"]?.ToString() ?? string.Empty),
                    new Claim("auth_type", "jwt")

                    // Add more claims as needed
                };

                var identity = new ClaimsIdentity(claims, "AuthenticationTypes.Federation");
                context.User = new ClaimsPrincipal(identity);
            }
            catch (Exception)
            {
                // Token validation failed - but don't return error here
                // Let the authorization attributes handle it
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Validates the provided API key against configured valid keys.
    /// </summary>
    /// <param name="apiKey">The API key to validate.</param>
    /// <returns>True if the API key is valid, false otherwise.</returns>
    private async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        // Get valid API keys from configuration
        string[] validApiKeys = _configuration.GetSection("Authentication:ApiKeys").Get<string[]>() ?? [];

        // For development/testing
        string? devApiKey = _configuration["Authentication:DevApiKey"];

        bool isOk = validApiKeys.Contains(apiKey) || (!string.IsNullOrWhiteSpace(devApiKey) && devApiKey == apiKey);

        return await Task.FromResult(isOk);
    }
}
