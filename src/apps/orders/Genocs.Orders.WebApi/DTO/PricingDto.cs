namespace Genocs.Orders.WebApi.DTO;

public class PricingDto
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
}