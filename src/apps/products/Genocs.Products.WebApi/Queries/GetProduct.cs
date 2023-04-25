using Genocs.Core.CQRS.Queries;
using Genocs.Products.WebApi.DTO;

namespace Genocs.Products.WebApi.Queries;

public class GetProduct : IQuery<ProductDto>
{
    public Guid ProductId { get; set; }
}