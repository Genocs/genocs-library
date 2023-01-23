namespace Genocs.QueryBuilder
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryItem
    {
    public QueryItem(string propertyName,
                    string propertyValue,
                    string propertyType = "string",
                    QueryOperator operatorType = QueryOperator.Equal)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        PropertyValue = propertyValue;
        OperatorType = operatorType;
    }

    public QueryItem(string propertyName,
                     string propertyValue,
                     bool parentCanBeNull,
                     string propertyType = "string",
                     QueryOperator operatorType = QueryOperator.Equal)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        PropertyValue = propertyValue;
        OperatorType = operatorType;
        ParentCanBeNull = parentCanBeNull;
    }

    public string PropertyName { get; set; }
    public string PropertyType { get; set; }
    public string PropertyValue { get; set; }
    public QueryOperator OperatorType { get; set; }
    public bool ParentCanBeNull { get; set; }
}
}