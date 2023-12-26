namespace Genocs.QueryBuilder;

/// <summary>
/// Query constants.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Default date format.
    /// </summary>
    public const string DATE_FORMAT = "yyyy-MM-dd";

    /// <summary>
    /// Default filtering date.
    /// </summary>
    public const string DATE_FILTERING = "UpdatedAt";

    /// <summary>
    /// Default property name.
    /// </summary>
    public const string DEFAULT_PROPERY_NAME = "IsDefault";
}

/// <summary>
/// Query operator types.
/// </summary>
public enum QueryOperator
{
    /// <summary>
    /// GreaterThan.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// GreaterThanOrEqual.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Equal.
    /// </summary>
    Equal,

    /// <summary>
    /// NotEqual.
    /// </summary>
    NotEqual,

    /// <summary>
    /// LessThan.
    /// </summary>
    LessThan,

    /// <summary>
    /// LessThanOrEqual.
    /// </summary>
    LessThanOrEqual
}