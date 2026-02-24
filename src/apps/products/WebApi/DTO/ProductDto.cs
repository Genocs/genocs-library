using Genocs.Common.Interfaces;

namespace Genocs.Products.WebApi.DTO;

public class ProductDto : IDto
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = default!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
}