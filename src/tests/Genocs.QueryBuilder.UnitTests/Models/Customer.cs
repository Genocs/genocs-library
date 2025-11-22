namespace Genocs.QueryBuilder.UnitTests.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerPinCode { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerOffice { get; set; }
    public string? LocationCode { get; set; }

    public Customer(int custid,
                    string custname,
                    string cusAddress,
                    string custtPin,
                    string custPhone,
                    string custEmail,
                    string custOffice,
                    string locCode)
    {
        CustomerId = custid;
        CustomerName = custname;
        CustomerAddress = cusAddress;
        CustomerPinCode = custtPin;
        CustomerPhoneNumber = custPhone;
        CustomerEmail = custEmail;
        CustomerOffice = custOffice;
        LocationCode = locCode;
    }
}
