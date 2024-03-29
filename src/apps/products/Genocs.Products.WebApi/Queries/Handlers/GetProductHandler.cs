using Genocs.Core.CQRS.Queries;
using Genocs.Persistence.MongoDb.Repositories.Mentor;
using Genocs.Products.WebApi.DTO;

namespace Genocs.Products.WebApi.Queries.Handlers;

public class GetProductHandler : IQueryHandler<GetProduct, ProductDto>
{
    private readonly IMongoRepository<Domain.Product, Guid> _repository;

    public GetProductHandler(IMongoRepository<Domain.Product, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> HandleAsync(GetProduct query, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetAsync(query.ProductId);

        return product is null
            ? null
            : new ProductDto { Id = product.Id, SKU = product.SKU, UnitPrice = product.UnitPrice };
    }
}