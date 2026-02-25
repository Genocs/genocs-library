using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.Masstransit.WebApi.Controllers;

/// <summary>
/// Controller for demonstrating Firebase authorization functionality.
/// This controller requires Firebase authentication for all endpoints.
/// </summary>
/// <remarks>
/// This controller serves as a demonstration of how to implement Firebase-based authorization
/// in ASP.NET Core Web API. All endpoints require a valid Firebase JWT token in the Authorization header.
/// </remarks>
[ApiController]
[Route("[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Firebase Authorization Demo")]
public class FirebaseAuthorizedController(ILogger<FirebaseAuthorizedController> logger) : ControllerBase
{
    private readonly ILogger<FirebaseAuthorizedController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieves authorization information for the authenticated user.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This endpoint demonstrates Firebase authorization by returning the authorization header
    /// from the authenticated request. It serves as a simple test to verify that Firebase
    /// authentication is working correctly.
    /// </para>
    /// <para>**Important:** This endpoint requires the user to have the 'Editor' role.</para>
    /// <para>Sample request:</para>
    /// <para>
    ///     GET /FirebaseAuthorized
    ///     Authorization: Bearer your-firebase-jwt-token-here.
    /// </para>
    /// </remarks>
    /// <returns>A string containing the authorization header value from the request.</returns>
    /// <response code="200">Returns the authorization header information successfully.</response>
    /// <response code="401">Unauthorized - Invalid or missing Firebase JWT token.</response>
    /// <response code="403">Forbidden - Valid token but user lacks the required 'Editor' role.</response>
    [HttpGet("")]
    [Authorize(Roles = "Editor")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuthorizedAsync()
    {
        _logger.LogInformation("Processing authorized request for user");

        return await Task.Run(() => Ok($"Done! Authorization is: {HttpContext.Request.Headers.Authorization}"));
    }

    /// <summary>
    /// Retrieves detailed user information and roles for debugging purposes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This endpoint returns detailed information about the authenticated user including
    /// all claims and roles assigned by the Firebase middleware. Useful for debugging
    /// authentication and authorization issues.
    /// </para>
    /// <para>
    /// Returns information such as:
    /// - Authentication status
    /// - User ID and email
    /// - Email verification status
    /// - Assigned roles
    /// - All JWT claims
    /// </para>
    /// <para>Sample request:</para>
    /// <para>
    ///     GET /FirebaseAuthorized/user-info
    ///     Authorization: Bearer your-firebase-jwt-token-here.
    /// </para>
    /// 
    /// </remarks>
    /// <returns>An object containing user details and assigned roles.</returns>
    /// <response code="200">Returns user information successfully.</response>
    /// <response code="401">Unauthorized - Invalid or missing Firebase JWT token.</response>
    [HttpGet("user-info")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetUserInfo()
    {
        var user = HttpContext.User;

        var userInfo = new
        {
            IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
            user.Identity?.AuthenticationType,
            user.Identity?.Name,
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Email = user.FindFirst(ClaimTypes.Email)?.Value,
            EmailVerified = user.FindFirst("email_verified")?.Value,
            Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            AllClaims = user.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            Clearance = false // Implement and check custom logic for clearance
        };

        _logger.LogInformation("User info requested for user {UserId}", userInfo.UserId);

        return Ok(userInfo);
    }

    /// <summary>
    /// Submits a request to join a team.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This endpoint allows authenticated users to submit a request to join a team.
    /// The request is logged and can be processed by team administrators.
    /// </para>
    /// <para>Sample request:</para>
    /// <para>
    ///     GET /FirebaseAuthorized/join-request
    ///     Authorization: Bearer your-firebase-jwt-token-here
    /// </para>
    /// 
    /// </remarks>
    /// <returns>A confirmation message indicating the join request was submitted.</returns>
    /// <response code="200">Returns confirmation that the join request was submitted successfully.</response>
    /// <response code="401">Unauthorized - Invalid or missing Firebase JWT token.</response>
    [HttpGet("join-request")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult PostJoinRequest()
    {
        // Sent a request to the team admin to join the team
        var user = HttpContext.User;
        _logger.LogInformation("Join request made by user {UserId}", user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        return Ok("PostJoinRequest");
    }
}
