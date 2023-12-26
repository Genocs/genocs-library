using System.Linq.Expressions;

namespace Genocs.QueryBuilder;

/// <summary>
/// DynamicQueryBuilder.
/// </summary>
public class DynamicQueryBuilder
{
    #region Private static members
    private static readonly string AndOperator = " and ";
    private static readonly string OrOperator = " or ";
    private static readonly string GtOperator = " gt ";
    private static readonly string LtOperator = " lt ";
    private static readonly string GtEOperator = " gte ";
    private static readonly string LtEOperator = " lte ";

    private static readonly string NotOperator = " not ";

    private static readonly string[] StringSeparators = new string[] { AndOperator, OrOperator, LtOperator, GtOperator, GtEOperator, LtEOperator };
    #endregion

    /// <summary>
    /// Builds the advanced search expression tree.
    /// This enables dynamic query on Following attributes
    /// MemberId, FirstName, LastName, Employer, Email, Address.City.
    /// It can be enabled on other fields as well, but i have just restricted as a sample ot these set of fields.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="searchItems">The search term.</param>
    /// <param name="sourceName">The search.</param>
    /// <returns></returns>
    public static Expression<Func<TSource, bool>> BuildAdvancedSearchExpressionTree<TSource>(List<QueryItem> searchItems, string sourceName)
    {
        ParameterExpression pe = Expression.Parameter(typeof(TSource), sourceName);
        Expression searchExpression = null;

        foreach (var searchItem in searchItems)
        {
            Expression expression = ExpressionBuilder<TSource>(searchItem, pe);

            // If you want to apply OR
            // searchExpression = searchExpression == null ? expression : Expression.OrElse(searchExpression, expression);

            // If you want to apply AND
            searchExpression = searchExpression == null ? expression : Expression.AndAlso(searchExpression, expression);
        }

        return Expression.Lambda<Func<TSource, bool>>(searchExpression, pe);
    }

    /// <summary>
    /// Builds the advanced search expression tree.
    /// This enables dynamic query on a single item.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="searchItem">The search term.</param>
    /// <param name="sourceName">The search.</param>
    /// <returns></returns>
    public static Expression<Func<TSource, bool>> BuildAdvancedSearchExpressionTree<TSource>(QueryItem searchItem, string sourceName)
    {
        ParameterExpression pe = Expression.Parameter(typeof(TSource), sourceName);
        Expression searchExpression = ExpressionBuilder<TSource>(searchItem, pe);
        return Expression.Lambda<Func<TSource, bool>>(searchExpression, pe);
    }

    private static Expression ExpressionBuilder<TSource>(QueryItem searchItem, ParameterExpression pe)
    {
        string[] results = searchItem.PropertyValue.Split(StringSeparators, StringSplitOptions.RemoveEmptyEntries);
        var operatorIndexes = GetListOfSortedOperatorIndexes(searchItem.PropertyValue);

        Expression expression;
        switch (searchItem.PropertyType.ToLower().Trim())
        {
            case "string":
                expression = QueryBuilder.ExpressionBuilder.GetExpressionString<TSource>(results, searchItem.PropertyValue, searchItem.PropertyName, operatorIndexes, pe, searchItem.ParentCanBeNull);
                break;
            case "int":
                expression = QueryBuilder.ExpressionBuilder.GetExpressionInt<TSource>(results, searchItem.PropertyName, searchItem.OperatorType, pe);
                break;
            case "numeric":
                expression = QueryBuilder.ExpressionBuilder.GetExpressionNumeric<TSource>(results, searchItem.PropertyName, searchItem.OperatorType, pe);
                break;
            case "date":
                expression = QueryBuilder.ExpressionBuilder.GetExpressionDate<TSource>(results, searchItem.PropertyName, searchItem.OperatorType, pe);
                break;
            case "bool":
                expression = QueryBuilder.ExpressionBuilder.GetExpressionBool<TSource>(results, searchItem.PropertyName, pe);
                break;
            default:
                expression = QueryBuilder.ExpressionBuilder.GetExpressionString<TSource>(results, searchItem.PropertyValue, searchItem.PropertyName, operatorIndexes, pe, searchItem.ParentCanBeNull);
                break;
        }

        return expression;

    }

    /// <summary>
    /// Gets the list of sorted operator indexes.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns></returns>
    private static List<OperatorIndexes> GetListOfSortedOperatorIndexes(string input)
    {
        input = input.ToLower();
        string[] result = input.Split(StringSeparators, StringSplitOptions.RemoveEmptyEntries);

        List<OperatorIndexes> operatorIndexes = new List<OperatorIndexes>();
        var andOperatorIndexes = AllIndexesOf(input, StringSeparators[0]);
        var orOperatorIndexes = AllIndexesOf(input, StringSeparators[1]);

        if (andOperatorIndexes.Count > 0)
        {
            operatorIndexes.AddRange(andOperatorIndexes);
        }

        if (orOperatorIndexes.Count > 0)
        {
            operatorIndexes.AddRange(orOperatorIndexes);
        }

        if (operatorIndexes.Count > 0)
        {
            var sorterOperatorIndexes = operatorIndexes.ToList().OrderBy(v => v.Index).ToList();
        }

        return operatorIndexes;
    }

    /// <summary>
    /// All the indexes of.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">the string to find may not be empty value.</exception>
    private static List<OperatorIndexes> AllIndexesOf(string str, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("the string to find may not be empty", "value");
        }

        List<OperatorIndexes> indexes = new List<OperatorIndexes>();
        for (int index = 0; ; index += value.Length)
        {
            index = str.IndexOf(value, index);
            if (index == -1)
                return indexes;
            indexes.Add(new OperatorIndexes
            {
                Index = index,
                Operator = value
            });
        }
    }
}

internal class OperatorIndexes
{
    public int Index { get; set; }
    public string Operator { get; set; }
}