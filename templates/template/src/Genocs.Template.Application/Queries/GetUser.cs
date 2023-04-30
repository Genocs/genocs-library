using Genocs.Core.CQRS.Queries;
using Genocs.Template.Application.DTO;

namespace Genocs.Template.Application.Queries;

public class GetUser : IQuery<UserDetailsDto>
{
    public Guid UserId { get; set; }
}