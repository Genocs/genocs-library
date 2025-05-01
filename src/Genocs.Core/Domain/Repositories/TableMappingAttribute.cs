namespace Genocs.Core.Domain.Repositories;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TableMappingAttribute(string tableName) : Attribute
{
    /// <summary>
    /// The Collection/Table name.
    /// </summary>
    public string Name { get; set; } = tableName;

    public readonly double Version = 1.0;
}
