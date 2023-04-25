using Genocs.Core.CQRS.Queries;
using Genocs.Identities.Application.DTO;

namespace Genocs.Identities.Application.Queries;

public class GetUser : IQuery<UserDetailsDto>
{
    public Guid UserId { get; set; }
}