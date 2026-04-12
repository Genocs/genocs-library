using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Genocs.Auth.Attributes;

/// <summary>
/// Authorization attribute that allows access with either valid JWT token or valid API key.
/// </summary>
/// <remarks>
/// This attribute enables dual authentication modes for endpoints:
/// 1. Standard JWT Bearer token authentication
/// 2. API key authentication via x-gnx-apikey header
/// Used primarily for system-to-system communication endpoints that need to support both authentication methods.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ApiKeyOrJwtAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if user is authenticated via any method
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            return; // Allow access
        }

        // Check for API key in header
        string? apiKey = context.HttpContext.Request.Headers["x-gnx-apikey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            // API key validation is handled in middleware
            // If we reach here with an API key, it means middleware didn't authenticate
            context.Result = new UnauthorizedObjectResult("Invalid API key");
            return;
        }

        // No valid authentication found
        context.Result = new UnauthorizedObjectResult("Authentication required. Provide either Bearer token or apikey.");
    }
}