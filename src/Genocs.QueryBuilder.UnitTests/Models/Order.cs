namespace Genocs.QueryBuilder.UnitTests.Models;

public class Order
{
    public int OrderId { get; set; }
    public string ProductName { get; set; }
    public string ProductCost { get; set; }
    public string ProductQunatity { get; set; }

    public Order(int orderid, string pName, string pCost, string Pquant)
    {
        OrderId = orderid;
        ProductCost = pCost;
        ProductQunatity = Pquant;
        ProductName = pName;
    }
}
