using Genocs.Core.Demo.Users.Application.Domain.Entities;

namespace Genocs.Core.Demo.Users.Application.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(AggregateId id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByNameAsync(string name);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}