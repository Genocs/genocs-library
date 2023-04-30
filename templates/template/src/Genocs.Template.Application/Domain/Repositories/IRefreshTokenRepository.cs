using Genocs.Template.Application.Domain.Entities;

namespace Genocs.Template.Application.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> GetAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
}