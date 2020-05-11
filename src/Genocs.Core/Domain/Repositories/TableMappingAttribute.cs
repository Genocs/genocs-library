namespace Genocs.Core.Domain.Repositories
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class TableMappingAttribute : System.Attribute
    {
        /// <summary>
        /// The Collection/Table name
        /// </summary>
        public string Name { get; set; }

        public double version;

        public TableMappingAttribute(string tableName)
        {
            Name = tableName;
            version = 1.0;
        }
    }
}
