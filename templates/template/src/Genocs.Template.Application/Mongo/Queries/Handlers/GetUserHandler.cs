using Genocs.Core.CQRS.Queries;
using Genocs.Persistence.MongoDb.Legacy;
using Genocs.Template.Application.DTO;
using Genocs.Template.Application.Mongo.Documents;
using Genocs.Template.Application.Queries;

namespace Genocs.Template.Application.Mongo.Queries.Handlers;

public class GetUserHandler : IQueryHandler<GetUser, UserDetailsDto>
{
    private readonly IMongoRepository<UserDocument, Guid> _userRepository;

    public GetUserHandler(IMongoRepository<UserDocument, Guid> userRepository)
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
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                Locked = user.Locked,
                Permissions = user.Permissions,
            };
    }
}