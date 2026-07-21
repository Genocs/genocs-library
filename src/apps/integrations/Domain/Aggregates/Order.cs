using Genocs.Core.Domain.Repositories;

namespace Genocs.Library.Demo.Domain.Aggregates;

[TableMapping("Orders")]
public class Order(string orderId, string userId, decimal amount, string currency) : BaseAggregate
{
    public string OrderId { get; init; } = orderId;
    public string UserId { get; init; } = userId;
    public decimal Amount { get; init; } = amount;
    public string Currency { get; init; } = currency;
}
