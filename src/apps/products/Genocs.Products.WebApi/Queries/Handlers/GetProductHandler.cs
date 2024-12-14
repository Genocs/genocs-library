using Genocs.Core.CQRS.Queries;
using Genocs.Persistence.MongoDb.Domain.Repositories;
using Genocs.Products.WebApi.DTO;

namespace Genocs.Products.WebApi.Queries.Handlers;

public class GetProductHandler : IQueryHandler<GetProduct, ProductDto>
{
    private readonly IMongoDbBaseRepository<Domain.Product, Guid> _repository;

    private static readonly Random _random = new();

    public GetProductHandler(IMongoDbBaseRepository<Domain.Product, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> HandleAsync(GetProduct query, CancellationToken cancellationToken = default)
    {
        int currentValue = _random.Next(0, 100);

        if (currentValue < 5)
        {
            throw new Exception("Random exception");
        }

        var product = await _repository.GetAsync(query.ProductId);

        return product is null
            ? null
            : new ProductDto { Id = product.Id, SKU = product.SKU, UnitPrice = product.UnitPrice };
    }
}