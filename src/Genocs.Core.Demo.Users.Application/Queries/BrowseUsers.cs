using Genocs.Core.CQRS.Queries;
using Genocs.Core.Demo.Users.Application.DTO;

namespace Genocs.Core.Demo.Users.Application.Queries;

public class BrowseUsers : PagedQueryBase, IQuery<PagedDto<UserDto>>
{
    public Guid? UserId { get; set; }
}