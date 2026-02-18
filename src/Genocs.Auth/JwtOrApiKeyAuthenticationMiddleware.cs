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
/// <para>
/// This middleware supports dual authentication modes:
/// </para>
/// <para>
/// 1. API Key authentication via x-gnx-apikey header for system-to-system communication.
/// </para>
/// <para>
/// 2. JWT Bearer token authentication for user authentication.
/// </para>
/// <para>
/// When API key is provided, it bypasses JWT validation and sets up API key-based claims.
/// </para>
/// </remarks>
public class JwtOrApiKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;

    public async Task Invoke(HttpContext context)
    {
        // Get the apiKey if any
        string? apiKey = context.Request.Headers["x-gnx-apikey"];

        // Get JWT authentication if any
        string? jwt = context.Request.Headers.Authorization;

        // Check if both authentication are in place
        if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(jwt))
        {
            // Invalid API key
            context.Response.StatusCode = 409;
            await context.Response.WriteAsync("Invalid Configuration! ApiKey and JWT token cannot be on the same time!");
            return;
        }

        // Check for API key authentication first
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

        // Check for JWT authentication
        if (jwt?.StartsWith("Bearer ") == true)
        {
            string token = jwt["Bearer ".Length..].Trim();

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
        // Check if enabled
        bool isEnabled = _configuration.GetValue<bool>("Authorization:Enabled");
        if (!isEnabled)
        {
            return await Task.FromResult(false);
        }

        // Get valid API keys from configuration
        string[] validApiKeys = _configuration.GetSection("Authorization:ApiKeys").Get<string[]>() ?? [];

        // For development/testing
        string? devApiKey = _configuration["Authorization:DevApiKey"];

        bool isOk = validApiKeys.Contains(apiKey) || (!string.IsNullOrWhiteSpace(devApiKey) && devApiKey == apiKey);

        return await Task.FromResult(isOk);
    }
}
