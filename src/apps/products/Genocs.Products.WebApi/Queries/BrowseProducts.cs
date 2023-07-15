using Genocs.Core.CQRS.Queries;
using Genocs.Products.WebApi.DTO;

namespace Genocs.Products.WebApi.Queries;

public class BrowseProducts : PagedQueryBase, IQuery<PagedResult<ProductDto>>
{

}