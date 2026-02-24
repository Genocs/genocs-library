namespace Genocs.QueryBuilder.UnitTests.Models;

public class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; }
    public List<Product> Products { get; set; }

    public Order(int orderId, int customerId)
    {

        OrderId = orderId;
        CustomerId = customerId;
        Products = [];
    }

    public Order(int orderId, int customerId, string pName, string pCost, int cost)
        : this(orderId, customerId)
    {
        Products.Add(new Product(pName, pCost, cost));
    }

    public void AddProduct(string pName, string pCost, int cost)
    {
        Products ??= [];
        Products.Add(new Product(pName, pCost, cost));
    }
}

public class Product
{
    public Product(string sKU, string? description, int cost)
    {
        SKU = sKU;
        Description = description;
        Cost = cost;
    }

    public string SKU { get; set; } = default!;
    public string? Description { get; set; }
    public int Cost { get; set; }

}
