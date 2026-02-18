using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Genocs.Auth;

internal sealed class DisabledAuthenticationPolicyEvaluator : IPolicyEvaluator
{
    /// <summary>
    /// This method is responsible for authenticating the user based on the provided
    /// authorization policy and HTTP context. In this implementation,
    /// it creates a successful authentication result with an empty claims principal,
    /// effectively bypassing any actual authentication logic. This allows all requests
    /// to be treated as authenticated, regardless of the presence of valid credentials.
    /// </summary>
    /// <param name="policy">The authorization policy to evaluate.</param>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authentication result.</returns>
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(), JwtBearerDefaults.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
    }

    /// <summary>
    /// This method is responsible for authorizing the user based on the provided
    /// authorization policy, authentication result, HTTP context, and resource.
    /// In this implementation, it always returns a successful authorization result,
    /// effectively bypassing any actual authorization logic. This allows all requests
    /// to be treated as authorized, regardless of the user's claims or roles.
    /// </summary>
    /// <param name="policy">The authorization policy to evaluate.</param>
    /// <param name="authenticationResult">The result of the authentication process.</param>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="resource">The resource being accessed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authorization result.</returns>
    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object resource)
    {
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }
}