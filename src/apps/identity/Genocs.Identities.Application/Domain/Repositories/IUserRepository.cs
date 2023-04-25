using Genocs.Identities.Application.Domain.Entities;

namespace Genocs.Identities.Application.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(AggregateId id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByNameAsync(string name);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}