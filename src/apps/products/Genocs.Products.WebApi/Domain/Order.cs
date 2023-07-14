using Genocs.Common.Types;

namespace Genocs.Products.WebApi.Domain;


/// <summary>
/// The product definition 
/// </summary>
public class Product : IIdentifiable<Guid>
{
    public Guid Id { get; private set; }
    public string SKU { get; private set; }
    public decimal UnitPrice { get; private set; }

    public Product(Guid id, string sku, decimal unitPrice)
    {
        Id = id;
        SKU = sku;
        UnitPrice = unitPrice;
    }

    public bool IsTransient()
    {
        throw new NotImplementedException();
    }
}