namespace Genocs.QueryBuilder.UnitTests.Models;

public class User
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }

    /// <summary>
    /// This is used as nullable to check the builder.
    /// </summary>
    public int? Childs { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public string? MobileNumber { get; set; }
    public string? MobilePrefix { get; set; }
    public string? MobileLanguage { get; set; }
    public string? CountryOfResidence { get; set; }
    public string? Currency { get; private set; }

    public Address? Address { get; set; }

    public object? this[string propertyName]
    {
        get { return GetType()?.GetProperty(propertyName)?.GetValue(this, null); }
        set { GetType()?.GetProperty(propertyName)?.SetValue(this, value, null); }
    }
}
