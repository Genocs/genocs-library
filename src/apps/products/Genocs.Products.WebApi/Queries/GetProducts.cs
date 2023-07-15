using Genocs.Core.CQRS.Queries;
using Genocs.Products.WebApi.DTO;

namespace Genocs.Products.WebApi.Queries;

public class GetProducts : IQuery<PagedResult<ProductDto>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }

}