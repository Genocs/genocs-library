using Genocs.Common.Domain.Entities;
using Genocs.Products.WebApi.Commands;

namespace Genocs.Products.WebApi.Domain;

/// <summary>
/// The product definition.
/// </summary>
public class Product(Guid id, string sku, decimal unitPrice) : IEntity<Guid>
{
    public Guid Id { get; private set; } = id;
    public string? Name { get; private set; }
    public string? Description { get; private set; }

    public string SKU { get; private set; } = sku;
    public decimal UnitPrice { get; private set; } = unitPrice;

    public Product(Guid id, string sku, decimal unitPrice, string? name, string? description)
        : this(id, sku, unitPrice)
    {
        Name = name;
        Description = description;
    }

    public bool IsTransient()
    {
        throw new NotImplementedException();
    }

    public static Product FromCommand(CreateProduct command)
    {
        return new Product(command.ProductId, command.SKU, command.UnitPrice, command.Name, command.Description);
    }
}