using Genocs.Identities.Application.DTO;
using Microsoft.Extensions.Caching.Memory;

namespace Genocs.Identities.Application.Services;

internal class TokenStorage : ITokenStorage
{
    private readonly IMemoryCache _cache;

    public TokenStorage(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Set(Guid commandId, AuthDto token)
        => _cache.Set(GetKey(commandId), token);

    public AuthDto Get(Guid commandId)
        => _cache.Get<AuthDto>(GetKey(commandId));

    private static string GetKey(Guid commandId)
        => $"users:tokens:{commandId}";
}