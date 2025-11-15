using Genocs.Core.Domain.Repositories;

namespace Genocs.Core.Demo.Domain.Aggregates;

[TableMapping("Users")]
public class User(string userId, string username, decimal age, string country) : BaseAggregate
{
    public string UserId { get; set; } = userId;
    public string Username { get; set; } = username;
    public decimal Age { get; set; } = age;
    public string Country { get; set; } = country;
}
