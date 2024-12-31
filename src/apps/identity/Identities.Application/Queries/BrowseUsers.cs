using Genocs.Core.CQRS.Queries;
using Genocs.Identities.Application.DTO;

namespace Genocs.Identities.Application.Queries;

public class BrowseUsers : PagedQueryBase, IQuery<PagedDto<UserDto>>
{
    public Guid? UserId { get; set; }
}