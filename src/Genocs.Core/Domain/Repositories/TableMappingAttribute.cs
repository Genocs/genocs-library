namespace Genocs.Core.Domain.Repositories;

/// <summary>
/// Attribute to specify the mapping of a class or struct to a database table or collection.
/// </summary>
/// <param name="tableName">The name of the table or collection.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TableMappingAttribute(string tableName) : Attribute
{
    /// <summary>
    /// The Collection/Table name.
    /// </summary>
    public string Name { get; set; } = tableName;

    /// <summary>
    /// The version of the mapping. This can be used to handle changes in the mapping over time.
    /// </summary>
    public readonly double Version = 1.0;
}
