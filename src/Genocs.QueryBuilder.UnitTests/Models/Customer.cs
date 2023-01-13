namespace Genocs.QueryBuilder.UnitTests.Models;

public class Customer
{
    public int CustomerID { get; set; }
    public int OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerPinCode { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerOffice { get; set; }
    public string? LocationCode { get; set; }

    public Customer(int custid,
                    int orderid,
                    string custname,
                    string cusAddress,
                    string custtPin,
                    string custPhone,
                    string custEmail,
                    string custOffice,
                    string locCode)
    {
        CustomerID = custid;
        OrderId = orderid;
        CustomerName = custname;
        CustomerAddress = cusAddress;
        CustomerPinCode = custtPin;
        CustomerPhoneNumber = custPhone;
        CustomerEmail = custEmail;
        CustomerOffice = custOffice;
        LocationCode = locCode;

    }

}
