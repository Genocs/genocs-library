using Genocs.Core.CQRS.Queries;
using Genocs.Persistence.MongoDb.Repositories;
using Genocs.Products.WebApi.Domain;
using Genocs.Products.WebApi.DTO;
using MongoDB.Driver;

namespace Genocs.Products.WebApi.Queries.Handlers;

public class BrowseProductsHandler : IQueryHandler<BrowseProducts, PagedResult<ProductDto>>
{
    private readonly IMongoDatabase _database;

    public BrowseProductsHandler(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<PagedResult<ProductDto>?> HandleAsync(BrowseProducts query, CancellationToken cancellationToken = default)
    {
        var result = await _database.GetCollection<Product>("products")
                                        .AsQueryable()
                                        .PaginateAsync(query);

        var pagedResult = PagedResult<ProductDto>.From(result, result.Items.Select(x => Map(x)));

        return pagedResult;
    }

    private static ProductDto Map(Product product)
        => new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            UnitPrice = product.UnitPrice
        };
}

