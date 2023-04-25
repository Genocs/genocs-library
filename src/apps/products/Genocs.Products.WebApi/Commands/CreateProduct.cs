using Genocs.Core.CQRS.Commands;

namespace Genocs.Products.WebApi.Commands;

public class CreateProduct : ICommand
{
    public Guid ProductId { get; }
    public string SKU { get; }
    public decimal UnitPrice { get; }


    public CreateProduct(Guid productId, string sku, decimal unitPrice)
    {
        ProductId = productId == Guid.Empty ? Guid.NewGuid() : productId;
        SKU = sku;
        UnitPrice = unitPrice;
    }
}