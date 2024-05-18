using Genocs.Orders.WebApi.DTO;

namespace Genocs.Orders.WebApi.Services;

/// <summary>
/// The Product WebApi client definition.
/// </summary>
public interface IProductServiceClient
{
    /// <summary>
    /// Get the product based on the productId.
    /// </summary>
    /// <param name="productId">The ProductId.</param>
    /// <returns>The Product Response.</returns>
    Task<ProductDto> GetAsync(Guid productId);
}