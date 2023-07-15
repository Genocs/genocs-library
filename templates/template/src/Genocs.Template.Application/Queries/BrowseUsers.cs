using Genocs.Core.CQRS.Queries;
using Genocs.Template.Application.DTO;

namespace Genocs.Template.Application.Queries;

public class BrowseUsers : PagedQueryBase, IQuery<PagedDto<UserDto>>
{
    public Guid? UserId { get; set; }
}