using Genocs.Core.Domain.Repositories;

namespace Genocs.Core.Demo.Domain.Aggregates;

[TableMapping("Orders")]
public class Order(string orderId, string userId, decimal amount, string currency) : BaseAggregate
{
    public string OrderId { get; set; } = orderId;
    public string UserId { get; set; } = userId;
    public decimal Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
}
