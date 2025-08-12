using System.Security.Claims;

namespace Genocs.Common.Interfaces;

/// <summary>
/// The CurrentUser interface.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The user id.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the user id.
    /// </summary>
    /// <returns>The default Id Type.</returns>
    DefaultIdType GetUserId();

    string? GetUserEmail();

    string? GetTenant();

    bool IsAuthenticated();

    /// <summary>
    /// Checks if the user is in a specific role.
    /// </summary>
    /// <param name="role">Role to check.</param>
    /// <returns>True in case of sucesful otherwise False.</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Gets the user claims.
    /// </summary>
    /// <returns>List of the claims.</returns>
    IEnumerable<Claim>? GetUserClaims();
}