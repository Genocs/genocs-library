using Genocs.Common.Interfaces;

namespace Genocs.Products.WebApi.DTO;

/// <summary>
/// The ProductDto class represents a data transfer object for a product, containing properties such as Id, SKU, Name, Description, and UnitPrice.
/// </summary>
public sealed class ProductDto : IDto
{
    /// <summary>
    /// Gets or initializes the unique identifier for the product.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or initializes the product SKU.
    /// </summary>
    public string SKU { get; init; } = default!;

    /// <summary>
    /// Gets or initializes the product name.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets or initializes the product description.
    /// </summary>
    public string Description { get; init; } = default!;

    /// <summary>
    /// The unit price of the product.
    /// </summary>
    public decimal UnitPrice { get; init; }
}