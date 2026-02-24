using Genocs.Common.Cqrs.Queries;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Genocs.Products.WebApi.DTO;

namespace Genocs.Products.WebApi.Queries.Handlers;

public class GetProductHandler : IQueryHandler<GetProduct, ProductDto>
{
    private readonly IMongoBaseRepository<Domain.Product, Guid> _repository;

    private static readonly Random _random = new();

    public GetProductHandler(IMongoBaseRepository<Domain.Product, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> HandleAsync(GetProduct query, CancellationToken cancellationToken = default)
    {
        int currentValue = _random.Next(0, 100);

        if (currentValue < 5)
        {
            throw new Exception("Intentional exception thrown for testing or demonstration purposes.");
        }

        var product = await _repository.GetAsync(query.ProductId, cancellationToken);

        return product is null
            ? null
            : new ProductDto
            {
                Id = product.Id,
                SKU = product.SKU,
                Name = product.Name,
                Description = product.Description,
                UnitPrice = product.UnitPrice
            };
    }
}