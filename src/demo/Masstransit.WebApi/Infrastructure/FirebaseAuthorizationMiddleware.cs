using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Genocs.Library.Demo.Masstransit.WebApi.Configurations;
using Microsoft.Extensions.Options;

namespace Genocs.Library.Demo.Masstransit.WebApi.Infrastructure;

/// <summary>
/// Enhanced Firebase authentication and authorization middleware.
/// Handles JWT token validation and role-based authorization with configurable options.
/// </summary>
public class FirebaseAuthorizationMiddleware(RequestDelegate next, ILogger<FirebaseAuthorizationMiddleware> logger, IOptions<FirebaseAuthorizationOptions> options)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<FirebaseAuthorizationMiddleware> _logger = logger;
    private readonly FirebaseAuthorizationOptions _options = options.Value;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await ProcessAuthenticationAsync(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Firebase authentication");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error during authentication");
            return;
        }

        await _next(context);
    }

    private async Task ProcessAuthenticationAsync(HttpContext context)
    {
        string authHeader = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // No token provided - let the default authorization handle it
            return;
        }

        string token = authHeader["Bearer ".Length..].Trim();

        if (string.IsNullOrEmpty(token))
        {
            await UnauthorizedResponse(context, "Empty token provided");
            return;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();

            // Validate token format
            if (!handler.CanReadToken(token))
            {
                await UnauthorizedResponse(context, "Invalid token format");
                return;
            }

            var jwtToken = handler.ReadJwtToken(token);
            var payload = jwtToken.Payload;

            // Validate token expiration
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                await UnauthorizedResponse(context, "Token expired");
                return;
            }

            // Extract user information from token
            string? userId = payload.TryGetValue("user_id", out object? userIdObj) ? userIdObj?.ToString() : string.Empty;
            string? name = payload.TryGetValue("name", out object? nameObj) ? nameObj?.ToString() : string.Empty;
            string? email = payload.TryGetValue("email", out object? emailObj) ? emailObj?.ToString() : string.Empty;
            bool emailVerified = payload.TryGetValue("email_verified", out object? emailVerifiedObj) &&
                              emailVerifiedObj is bool verified && verified;

            if (string.IsNullOrEmpty(userId))
            {
                await UnauthorizedResponse(context, "Invalid user information in token");
                return;
            }

            // Check email verification requirement
            if (_options.RequireEmailVerification && !emailVerified)
            {
                // In demo mode, we'll allow unverified emails but log a warning
                if (!_options.DemoMode)
                {
                    await UnauthorizedResponse(context, "Email not verified");
                    return;
                }

                _logger.LogWarning("Demo mode: Allowing unverified email for user {UserId}", userId);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, name ?? string.Empty),
                new(ClaimTypes.Email, email ?? string.Empty),
                new("firebase_user_id", userId),
                new("email_verified", emailVerified.ToString().ToLower())
            };

            // Add roles based on configuration
            var userRoles = GetUserRoles(userId, email, emailVerified, payload);
            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add additional custom claims
            AddCustomClaims(claims, payload);

            var identity = new ClaimsIdentity(claims, "Firebase");
            context.User = new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            await UnauthorizedResponse(context, "Invalid token");
        }
    }

    /// <summary>
    /// Determines user roles based on configuration and business logic.
    /// </summary>
    private List<string> GetUserRoles(string userId, string email, bool emailVerified, IDictionary<string, object> payload)
    {
        var roles = new HashSet<string>();

        // Add default roles
        foreach (string defaultRole in _options.DefaultRoles)
        {
            roles.Add(defaultRole);
        }

        // Demo mode - assign Editor role to all users (even with unverified emails)
        if (_options.DemoMode)
        {
            roles.Add("Editor");
            _logger.LogDebug("Demo mode: Added Editor role for user {UserId}", userId);
        }

        // Check specific email mappings (allow even if email not verified in demo mode)
        if (!string.IsNullOrEmpty(email) && _options.EmailRoles.TryGetValue(email, out var emailRoles))
        {
            if (emailVerified || _options.DemoMode)
            {
                foreach (string role in emailRoles)
                {
                    roles.Add(role);
                }
            }
            else
            {
                _logger.LogWarning("User {Email} has specific roles configured but email is not verified", email);
            }
        }

        // Check email domain mappings
        if (!string.IsNullOrEmpty(email))
        {
            foreach (var domainMapping in _options.EmailDomainRoles)
            {
                if (email.EndsWith(domainMapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    if (emailVerified || _options.DemoMode)
                    {
                        foreach (string role in domainMapping.Value)
                        {
                            roles.Add(role);
                        }
                    }
                }
            }
        }

        // Check for custom claims in Firebase token
        if (payload.TryGetValue("custom_claims", out var customClaimsObj))
        {
            try
            {
                string? customClaimsJson = customClaimsObj.ToString();
                if (!string.IsNullOrEmpty(customClaimsJson))
                {
                    var customClaims = JsonSerializer.Deserialize<Dictionary<string, object>>(customClaimsJson);
                    if (customClaims != null && customClaims.TryGetValue("roles", out var rolesObj))
                    {
                        if (rolesObj is JsonElement rolesElement && rolesElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var roleElement in rolesElement.EnumerateArray())
                            {
                                if (roleElement.ValueKind == JsonValueKind.String)
                                {
                                    string? role = roleElement.GetString();
                                    if (!string.IsNullOrEmpty(role))
                                    {
                                        roles.Add(role);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse custom claims for user {UserId}", userId);
            }
        }

        return roles.ToList();
    }

    /// <summary>
    /// Adds additional custom claims based on the Firebase token payload.
    /// </summary>
    private static void AddCustomClaims(List<Claim> claims, IDictionary<string, object> payload)
    {
        // Add issuer information
        if (payload.TryGetValue("iss", out object? issuer))
        {
            claims.Add(new Claim("firebase_issuer", issuer?.ToString() ?? string.Empty));
        }

        // Add audience information
        if (payload.TryGetValue("aud", out object? audience))
        {
            claims.Add(new Claim("firebase_audience", audience?.ToString() ?? string.Empty));
        }

        // Add auth time
        if (payload.TryGetValue("auth_time", out object? authTime))
        {
            claims.Add(new Claim("auth_time", authTime?.ToString() ?? string.Empty));
        }
    }

    private async Task UnauthorizedResponse(HttpContext context, string message)
    {
        _logger.LogWarning("Authentication failed: {Message}", message);
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var response = new { error = "Unauthorized", message };
        string jsonResponse = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(jsonResponse);
    }
}