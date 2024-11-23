using Genocs.Core.Domain.Entities;

namespace Genocs.Orders.WebApi.Domain;

public class Order : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }

    public Order(Guid id, Guid customerId, decimal totalAmount)
    {
        Id = id;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }

    public bool IsTransient()
    {
        throw new NotImplementedException();
    }
}