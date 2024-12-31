using Genocs.Core.CQRS.Commands;

namespace Genocs.Products.WebApi.Commands;

public class CreateProduct(Guid productId, string sku, decimal unitPrice, string? name, string? description) : ICommand
{
    public Guid ProductId { get; } = productId == Guid.Empty ? Guid.NewGuid() : productId;
    public string SKU { get; } = sku;
    public decimal UnitPrice { get; } = unitPrice;
    public string? Name { get; } = name;
    public string? Description { get; } = description;
}