using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Genocs.Library.Demo.WebApi.Configurations;

namespace Genocs.Library.Demo.WebApi.Infrastructure;

/// <summary>
/// Claims transformation service that adds roles to Firebase-authenticated users.
/// </summary>
public class FirebaseClaimsTransformation(
    ILogger<FirebaseClaimsTransformation> logger,
    IOptions<FirebaseAuthorizationOptions> options) : IClaimsTransformation
{
    private readonly ILogger<FirebaseClaimsTransformation> _logger = logger;
    private readonly FirebaseAuthorizationOptions _options = options.Value;

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Only process if user is authenticated and doesn't already have roles
        if (principal.Identity?.IsAuthenticated == false || principal.HasClaim(ClaimTypes.Role, string.Empty))
        {
            return Task.FromResult(principal);
        }

        // Check if this is a Firebase token by looking for firebase-specific claims
        var userIdClaim = principal.FindFirst("user_id") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
        var emailClaim = principal.FindFirst("email") ?? principal.FindFirst(ClaimTypes.Email);
        var emailVerifiedClaim = principal.FindFirst("email_verified");

        if (userIdClaim == null)
        {
            return Task.FromResult(principal);
        }

        string userId = userIdClaim.Value;
        string email = emailClaim?.Value ?? string.Empty;
        bool emailVerified = emailVerifiedClaim?.Value == "true";

        _logger.LogDebug("Transforming claims for user {UserId} with email {Email}", userId, email);

        // Get roles based on configuration
        var userRoles = GetUserRoles(userId, email, emailVerified);

        if (userRoles.Count != 0)
        {
            // Create a new identity with the additional role claims

            if (principal.Identity is ClaimsIdentity identity)
            {
                foreach (string role in userRoles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                _logger.LogInformation("Added roles {Roles} to user {UserId}", string.Join(", ", userRoles), userId);
            }
        }

        return Task.FromResult(principal);
    }

    private List<string> GetUserRoles(string userId, string email, bool emailVerified)
    {
        var roles = new HashSet<string>();

        // Add default roles
        foreach (string defaultRole in _options.DefaultRoles)
        {
            roles.Add(defaultRole);
        }

        // Demo mode - assign Editor role to all users
        if (_options.DemoMode)
        {
            roles.Add("Editor");
            _logger.LogDebug("Demo mode: Added Editor role for user {UserId}", userId);
        }

        // Check specific user ID mappings
        if (emailVerified)
        {
            // Add Custom roles based on your business logic
        }

        // Check specific email mappings
        if (!string.IsNullOrEmpty(email) && _options.EmailRoles.TryGetValue(email, out var emailRoles))
        {
            if (emailVerified || _options.DemoMode)
            {
                foreach (string role in emailRoles)
                {
                    roles.Add(role);
                }

                _logger.LogDebug("Added email-specific roles for {Email}: {Roles}", email, string.Join(", ", emailRoles));
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

                        _logger.LogDebug("Added domain-specific roles for {Email} (domain: {Domain}): {Roles}", email, domainMapping.Key, string.Join(", ", domainMapping.Value));
                    }
                }
            }
        }

        return [.. roles];
    }
}