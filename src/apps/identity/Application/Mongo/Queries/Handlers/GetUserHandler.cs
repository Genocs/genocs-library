using Genocs.Common.Cqrs.Queries;
using Genocs.Identities.Application.DTO;
using Genocs.Identities.Application.Mongo.Documents;
using Genocs.Identities.Application.Queries;
using Genocs.Persistence.MongoDB.Domain.Repositories;

namespace Genocs.Identities.Application.Mongo.Queries.Handlers;

public class GetUserHandler : IQueryHandler<GetUser, UserDetailsDto>
{
    private readonly IMongoBaseRepository<UserDocument, Guid> _userRepository;

    public GetUserHandler(IMongoBaseRepository<UserDocument, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDetailsDto?> HandleAsync(GetUser query, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(query.UserId);

        return user is null
            ? null
            : new UserDetailsDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Roles = user.Roles,
                CreatedAt = user.CreatedAt,
                Locked = user.Locked,
                Permissions = user.Permissions,
            };
    }
}