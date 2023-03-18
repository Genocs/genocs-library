using Convey.CQRS.Queries;
using Genocs.Core.Demo.Users.Application.DTO;

namespace Genocs.Core.Demo.Users.Application.Queries;

public class GetUser : IQuery<UserDetailsDto>
{
    public Guid UserId { get; set; }
}