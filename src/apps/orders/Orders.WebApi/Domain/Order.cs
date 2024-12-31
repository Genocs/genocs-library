using Genocs.Core.Domain.Entities;

namespace Genocs.Orders.WebApi.Domain;

public class Order(Guid id, Guid customerId, List<OrderItem> items ) : IEntity<Guid>
{
    public Guid Id { get; private set; } = id;
    public Guid CustomerId { get; private set; } = customerId;
    public List<OrderItem> Items { get; private set; } = items;
    public decimal TotalAmount { get; private set; } = items.Sum(c => c.UnitPrice * c.Quantity);

    public bool IsTransient()
    {
        throw new NotImplementedException();
    }
}

public class OrderItem(Guid productId, decimal unitPrice, int quantity)
{
    public Guid ProductId { get; private set; } = productId;
    public decimal UnitPrice { get; private set; } = unitPrice;
    public int Quantity { get; private set; } = quantity;
}