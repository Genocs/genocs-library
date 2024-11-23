using Genocs.QueryBuilder.UnitTests.Models;
using System.Linq.Dynamic.Core;
using Xunit;

namespace Genocs.QueryBuilder.UnitTests.DynamicQuery;

public class InMemoryBasicDynamicQueriesUnitTests
{
    [Fact]
    public void InMemoryLinqTest()
    {
        var liCust = new List<Customer>();

        Customer oCust = new Customer(001, "Devesh", "Ghaziabad",
        "250301", "9891586890", "devesh.akgec@gmail.com", "Genpact", "3123000");
        liCust.Add(oCust);
        oCust = new Customer(002, "NIKHIL", "NOIDA", "250201",
        "xxx9892224", "devesh.akgec@gmail.com", "X-vainat", "4123001");
        liCust.Add(oCust);
        oCust = new Customer(003, "Shruti", "NOIDA", "25001",
        "xxx0002345", "devesh.akgec@gmail.com", "Genpact", "5123002");
        liCust.Add(oCust);
        oCust = new Customer(004, "RAJ", "DELHI", "2500133",
        "xxx9898907", "devesh.akgec@gmail.com", "HCL", "6123003");
        liCust.Add(oCust);
        oCust = new Customer(005, "Shubham", "Patna", "250013",
        "x222333xx3", "devesh.akgec@gmail.com", "Genpact", "6123004");
        liCust.Add(oCust);

        // Order data
        var liOrder = new List<Order>();
        Order order = new Order(123000, 003);
        order.Products.Add(new Product("Noika Lumia", "Noika Lumia", 150));
        order.Products.Add(new Product("Moto G", "Moto G", 2500));
        order.Products.Add(new Product("Intext Mobile", "Intext Mobile", 7000));
        liOrder.Add(order);

        //Order oOrder = new Order(123000, "Noika Lumia", "7000", "2");
        //liOrder.Add(oOrder);
        //oOrder = new Order(123001, "Moto G", "17000", "1");
        //liOrder.Add(oOrder);
        //oOrder = new Order(123002, "Intext Mobile", "7000", "1");
        //liOrder.Add(oOrder);
        //oOrder = new Order(123001, "Celkom GX898", "2500", "1");
        //liOrder.Add(oOrder);
        //oOrder = new Order(123001, "Micromax", "1000", "10");
        //liOrder.Add(oOrder);
        //oOrder = new Order(222, "NOIKA Asha", "2500", "1");
        //liOrder.Add(oOrder);
        //oOrder = new Order(22212, "Iphone", "1000", "10");
        //liOrder.Add(oOrder);

        // Query Sintax
        var result = (from c in liCust
                      join o in liOrder
                      on c.CustomerId equals o.CustomerId
                      select new
                      {
                          c,
                          o
                      }).AsQueryable();

        string selectStatement = "Where(x => x.OrderId = 123001)";
        IQueryable iq = result.Select(x => selectStatement);

        var resultList = iq.ToDynamicList();
    }
}
