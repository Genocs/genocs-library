namespace Genocs.QueryBuilder
{
    public static class Constants
    {
        public const string DATE_FORMAT = "yyyy-MM-dd";
        public const string DATE_FILTERING = "UpdatedAt";
        public const string DEFAULT_PROPERY_NAME = "IsDefault";
    }

    public enum OperatorTypes
    {
        GreaterThan,
        GreaterThanOrEqual,
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual
    }
}