using System.Linq.Expressions;

namespace Genocs.QueryBuilder;

public static partial class ExpressionBuilder
{
    /// <summary>
    /// Gets the expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="searchTerms">The search terms.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="operatorType">The operatorType.</param>
    /// <param name="pe">The parameter expression.</param>
    /// <returns></returns>
    internal static Expression GetExpressionInt<TSource>(
                                                            string[] searchTerms,
                                                            string propertyName,
                                                            QueryOperator operatorType,
                                                            ParameterExpression pe)
    {
        // Compose the expression tree that represents the parameter to the predicate.
        Expression propertyExp = pe;
        foreach (string? member in propertyName.Split('.'))
        {
            propertyExp = Expression.PropertyOrField(propertyExp, member);
        }

        ConstantExpression constantExpression = Expression.Constant(int.Parse(searchTerms[0].ToLower()));

        var nullCheck = NormalizeNullable(propertyExp, constantExpression);
        Expression searchExpression = operatorType switch
        {
            QueryOperator.GreaterThan => Expression.GreaterThan(nullCheck.Item1, nullCheck.Item2),
            QueryOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(nullCheck.Item1, nullCheck.Item2),
            QueryOperator.LessThan => Expression.LessThan(nullCheck.Item1, nullCheck.Item2),
            QueryOperator.LessThanOrEqual => Expression.LessThanOrEqual(nullCheck.Item1, nullCheck.Item2),
            QueryOperator.NotEqual => Expression.NotEqual(nullCheck.Item1, nullCheck.Item2),
            _ => Expression.Equal(nullCheck.Item1, nullCheck.Item2),
        };
        return searchExpression;
    }
}