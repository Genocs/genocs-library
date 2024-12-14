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
    /// 
    /// </summary>
    /// <param name="policy"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(), JwtBearerDefaults.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="policy"></param>
    /// <param name="authenticationResult"></param>
    /// <param name="context"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object resource)
    {
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }
}