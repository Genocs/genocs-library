using Genocs.Core.Domain.Repositories;

namespace Genocs.Library.Demo.Domain.Aggregates;

[TableMapping("Users")]
public class User(string userId, string username, decimal age, string country) : BaseAggregate
{
    public string UserId { get; init; } = userId;
    public string Username { get; init; } = username;
    public decimal Age { get; init; } = age;
    public string Country { get; init; } = country;
}
