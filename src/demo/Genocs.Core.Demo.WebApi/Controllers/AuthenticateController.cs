using Genocs.Auth.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Genocs.Core.Demo.WebApi.Controllers;

/// <summary>
/// Authentication controller for API key and token validation.
/// </summary>
/// <remarks>
/// Provides endpoints for authentication validation and user information retrieval.
/// Supports both API key authentication via x-gnx-apikey header and JWT Bearer token authentication.
/// Used for authentication testing, token validation, and system integration verification.
/// The authenticate endpoint is accessible with API key authentication without additional restrictions.
/// </remarks>
[Produces("application/json")]
[Route("[controller]")]
public class AuthenticateController : ControllerBase
{
    /// <summary>
    /// Validate authentication and return user information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Authenticate endpoint with no auth restriction when API key is provided.
    /// This endpoint validates the current authentication state and returns user information.
    /// Supports both API key authentication (via x-gnx-apikey header) and JWT Bearer token authentication.
    /// When API key is provided, bypasses standard JWT validation requirements.
    /// Returns different user information based on the authentication method used.
    /// Essential for system integration testing and authentication verification workflows.
    /// </para>
    /// <para>
    /// Authentication Methods:
    /// 1. API Key: Include 'x-gnx-apikey' header with valid API key
    /// 2. JWT Token: Include 'Authorization: Bearer {token}' header
    /// </para>
    /// </remarks>
    /// <response code="200">Authentication successful with user information.</response>
    /// <response code="401">Authentication failed or missing credentials.</response>
    /// <returns>Authentication status and user information based on the authentication method.</returns>
    [ApiKeyOrJwtAuthorize]
    [HttpGet]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Authenticate()
    {
        var user = HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return Unauthorized("Authentication required");
        }

        string authType = user.FindFirst("auth_type")?.Value ?? "unknown";
        string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? userName = user.FindFirst(ClaimTypes.Name)?.Value;

        var response = new AuthenticationResponse
        {
            IsAuthenticated = true,
            AuthenticationType = authType,
            UserId = userId,
            UserName = userName,
            Claims = [.. user.Claims.Select(c => new ClaimInfo
            {
                Type = c.Type,
                Value = c.Value
            })]
        };

        return Ok(response);
    }

    /// <summary>
    /// Get current user information (requires standard JWT authentication).
    /// </summary>
    /// <remarks>
    /// Get user information with standard JWT authentication requirement.
    /// This endpoint requires valid JWT Bearer token authentication and does not accept API key authentication.
    /// Returns detailed user information from the JWT token claims.
    /// Used for user profile access and authenticated user operations.
    /// Demonstrates standard authorization requirements compared to the flexible authenticate endpoint.
    /// </remarks>
    /// <response code="200">User information retrieved successfully.</response>
    /// <response code="401">JWT authentication required.</response>
    /// <returns>Current user information from JWT token.</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetUserInfo()
    {
        var user = HttpContext.User;
        string? authType = user.Identity?.AuthenticationType;

        // Only allow JWT authentication for this endpoint
        if (authType != "AuthenticationTypes.Federation")
        {
            return Unauthorized("JWT Bearer token authentication required");
        }

        var response = new UserInfoResponse
        {
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            UserName = user.FindFirst(ClaimTypes.Name)?.Value,
            AuthenticationType = authType
        };

        return Ok(response);
    }

    /// <summary>
    /// Get current user information (requires standard JWT authentication).
    /// </summary>
    /// <remarks>
    /// Get user information with standard JWT authentication requirement.
    /// This endpoint requires valid JWT Bearer token authentication and does not accept API key authentication.
    /// Returns detailed user information from the JWT token claims.
    /// Used for user profile access and authenticated user operations.
    /// Demonstrates standard authorization requirements compared to the flexible authenticate endpoint.
    /// </remarks>
    /// <response code="200">User information retrieved successfully.</response>
    /// <response code="401">JWT authentication required.</response>
    /// <response code="404">Authorization fail. </response>
    /// <returns>Current user information from JWT token.</returns>
    [Authorize(Roles = "gnx-users")]
    [HttpGet("gnx-user")]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetUserInRoleInfo()
    {
        var user = HttpContext.User;
        string? authType = user.Identity?.AuthenticationType;

        // Only allow JWT authentication for this endpoint
        if (authType != "AuthenticationTypes.Federation")
        {
            return Unauthorized("JWT Bearer token authentication required");
        }

        var response = new UserInfoResponse
        {
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            UserName = user.FindFirst(ClaimTypes.Name)?.Value,
            AuthenticationType = authType
        };

        return Ok(response);
    }
}
