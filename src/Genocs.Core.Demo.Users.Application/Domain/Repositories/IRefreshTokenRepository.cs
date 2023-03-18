using Genocs.Core.Demo.Users.Application.Domain.Entities;

namespace Genocs.Core.Demo.Users.Application.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> GetAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
}