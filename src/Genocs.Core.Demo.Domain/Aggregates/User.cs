using Genocs.Core.Domain.Repositories;

namespace Genocs.Core.Demo.Domain.Aggregates;

[TableMapping("Users")]
public class User : BaseAggregate
{
    public string UserId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public decimal Age { get; set; }
    public string Country { get; set; } = default!;

    public User(string userId, string username, decimal age, string country)
    {
        UserId = userId;
        Username = username;
        Age = age;
        Country = country;
    }
}
