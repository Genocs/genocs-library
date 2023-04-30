using Genocs.Persistence.MongoDb.Legacy;
using Genocs.Template.Application.Domain.Entities;
using Genocs.Template.Application.Domain.Repositories;
using Genocs.Template.Application.Mongo.Documents;

namespace Genocs.Template.Application.Mongo.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoRepository<UserDocument, Guid> _repository;

    public UserRepository(IMongoRepository<UserDocument, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<User?> GetAsync(AggregateId id)
    {
        var document = await _repository.GetAsync(id);
        return document?.ToEntity();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var document = await _repository.GetAsync(x => x.Email == email.ToLowerInvariant());
        return document?.ToEntity();
    }

    public async Task<User?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var document = await _repository.GetAsync(x => x.Name == name);
        return document?.ToEntity();
    }

    public Task AddAsync(User user)
        => _repository.AddAsync(new UserDocument(user));

    public Task UpdateAsync(User user)
        => _repository.UpdateAsync(new UserDocument(user));
}