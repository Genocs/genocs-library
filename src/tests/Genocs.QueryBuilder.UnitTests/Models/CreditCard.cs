namespace Genocs.QueryBuilder.UnitTests.Models;

public class CreditCard
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Schema { get; set; }
    public string? BIN { get; set; }
    public bool IsDefault { get; set; }
}
