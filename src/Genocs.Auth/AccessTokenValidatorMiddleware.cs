using Genocs.Auth.Configurations;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Genocs.Auth;

/// <summary>
/// The access token validator middleware.
/// </summary>
public class AccessTokenValidatorMiddleware : IMiddleware
{
    private readonly IAccessTokenService _accessTokenService;
    private readonly IEnumerable<string> _endpoints;

    /// <summary>
    /// The AccessTokenValidatorMiddleware constructor.
    /// </summary>
    /// <param name="accessTokenService">The access token service.</param>
    /// <param name="options">The options.</param>
    public AccessTokenValidatorMiddleware(IAccessTokenService accessTokenService, JwtOptions options)
    {
        _accessTokenService = accessTokenService;
        _endpoints = options.AllowAnonymousEndpoints ?? Enumerable.Empty<string>();
    }

    /// <summary>
    /// The InvokeAsync method.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <param name="next">The request delegate.</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;

        if (_endpoints.Contains(path))
        {
            await next(context);

            return;
        }

        if (await _accessTokenService.IsCurrentActiveToken())
        {
            await next(context);

            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    }
}