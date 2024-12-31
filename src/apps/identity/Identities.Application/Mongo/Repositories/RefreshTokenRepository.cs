using Genocs.Identities.Application.Domain.Entities;
using Genocs.Identities.Application.Domain.Repositories;
using Genocs.Identities.Application.Mongo.Documents;
using Genocs.Persistence.MongoDb.Domain.Repositories;

namespace Genocs.Identities.Application.Mongo.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoDbBaseRepository<RefreshTokenDocument, Guid> _repository;

    public RefreshTokenRepository(IMongoDbBaseRepository<RefreshTokenDocument, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<RefreshToken> GetAsync(string token)
    {
        var refreshToken = await _repository.GetAsync(x => x.Token == token);
        return refreshToken?.ToEntity();
    }

    public Task AddAsync(RefreshToken refreshToken)
        => _repository.AddAsync(new RefreshTokenDocument(refreshToken));

    public Task UpdateAsync(RefreshToken refreshToken)
        => _repository.UpdateAsync(new RefreshTokenDocument(refreshToken));
}