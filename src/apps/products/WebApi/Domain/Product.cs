using Genocs.Common.Domain.Entities;
using Genocs.Products.WebApi.Commands;

namespace Genocs.Products.WebApi.Domain;

/// <summary>
/// The product definition.
/// </summary>
public class Product(Guid id, string sku, string name, string description, decimal unitPrice) : IEntity<Guid>
{
    /// <summary>
    /// The product identifier.
    /// </summary>
    public Guid Id { get; private set; } = id;

    /// <summary>
    /// The product name.
    /// </summary>
    public string Name { get; private set; } = name;

    /// <summary>
    /// The product description.
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// The product SKU.
    /// </summary>
    public string SKU { get; private set; } = sku;

    /// <summary>
    /// The product unit price.
    /// </summary>
    public decimal UnitPrice { get; private set; } = unitPrice;

    /// <summary>
    /// Determines whether the entity is transient (not yet persisted).
    /// </summary>
    /// <returns>true if the entity is transient; otherwise, false.</returns>
    public bool IsTransient()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Helps to create a new product from the given command.
    /// </summary>
    /// <param name="command">The create product command.</param>
    /// <returns>The created product.</returns>
    public static Product FromCommand(CreateProduct command)
    {
        return new Product(command.ProductId, command.SKU, command.Name, command.Description, command.UnitPrice);
    }
}