using Genocs.Auth.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Genocs.Auth.Services;

/// <summary>
/// This service allows to validate JWT Token in real-time.
/// In this way tokens can be invalidated and the effect shall be immediate.
/// </summary>
internal sealed class InMemoryAccessTokenService(IMemoryCache cache, IHttpContextAccessor httpContextAccessor, JwtOptions jwtOptions) : IAccessTokenService
{
    private readonly IMemoryCache _cache = cache;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly TimeSpan _expires = jwtOptions.Expiry ?? TimeSpan.FromMinutes(jwtOptions.ExpiryMinutes);

    public bool IsCurrentActiveToken()
        => IsActive(GetCurrent());

    public void DeactivateCurrent()
        => Deactivate(GetCurrent());

    public bool IsActive(string token)
        => string.IsNullOrWhiteSpace(_cache.Get<string>(GetKey(token)));

    public void Deactivate(string token)
    {
        _cache.Set(GetKey(token), "revoked", new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _expires
        });
    }

    private string GetCurrent()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization;

        if (authorizationHeader is null)
        {
            return string.Empty;
        }

        return authorizationHeader.Value == StringValues.Empty
            ? string.Empty
            : authorizationHeader.Value.Single()?.Split(' ').Last();
    }

    private static string GetKey(string token) => $"blacklisted-tokens:{token}";
}