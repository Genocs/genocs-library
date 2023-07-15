using Genocs.Core.CQRS.Queries;
using Genocs.Persistence.MongoDb.Repositories;
using Genocs.Products.WebApi.Domain;
using Genocs.Products.WebApi.DTO;

namespace Genocs.Products.WebApi.Queries.Handlers;

/*

public class GetProductsHandler : IQueryHandler<GetProducts, PagedResult<ProductDto>>
{
    private readonly IMongoDbRepository<Product> _repository;

    public GetProductsHandler(IMongoDbRepository<Product> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<PagedResult<ProductDto>?> HandleAsync(GetProducts query, CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllListAsync();

        return null;

        //return products is null
        //    ? null
        //    : new ProductDto { Id = product.Id, SKU = product.SKU, UnitPrice = product.UnitPrice };
    }
}

*/