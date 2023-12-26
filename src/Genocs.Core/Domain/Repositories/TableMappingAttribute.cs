namespace Genocs.Core.Domain.Repositories;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TableMappingAttribute : Attribute
{
    /// <summary>
    /// The Collection/Table name.
    /// </summary>
    public string Name { get; set; }

    public readonly double Version;

    public TableMappingAttribute(string tableName)
    {
        Name = tableName;
        Version = 1.0;
    }
}
