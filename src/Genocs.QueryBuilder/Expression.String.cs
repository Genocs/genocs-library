using System.Linq.Expressions;

namespace Genocs.QueryBuilder;

/// <summary>
/// ExpressionBuilder class.
/// </summary>
public static partial class ExpressionBuilder
{
    /// <summary>
    /// Gets the expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="searchTerms">The search terms.</param>
    /// <param name="keyword">The keyword.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="operatorIndexes">The operator indexes.</param>
    /// <param name="pe">The parameter expression.</param>
    /// <param name="addParentObjectNullCheck">if set to <c>true</c> [add parent object null check].</param>
    /// <returns>The expression.</returns>
    internal static Expression GetExpressionString<TSource>(
                                                    string[] searchTerms,
                                                    string keyword,
                                                    string propertyName,
                                                    List<OperatorIndexes> operatorIndexes,
                                                    ParameterExpression pe,
                                                    bool addParentObjectNullCheck)
    {
        // Compose the expression tree that represents the parameter to the predicate.
        Expression propertyExp = pe;
        string[] members = propertyName.Split('.');
        foreach (string member in members)
        {
            propertyExp = Expression.PropertyOrField(propertyExp, member);
        }

        Expression? searchExpression = null;
        MethodCallExpression left = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

        var method = typeof(string).GetMethod("Contains", [typeof(string)]);

        if (method != null)
        {
            for (int count = 0; count < searchTerms.Length; count++)
            {
                string searchTerm = searchTerms[count].ToLower();
                searchTerm = searchTerm.Replace("*", string.Empty);
                searchTerm = searchTerm.Replace("\"", string.Empty);
                Expression rightExpression;
                Expression methodCallExpression;
                if (searchTerm.Contains(NotOperator.TrimStart()))
                {
                    searchTerm = searchTerm.Replace(NotOperator.TrimStart(), string.Empty).Trim();
                    rightExpression = Expression.Constant(searchTerm);
                    methodCallExpression = Expression.Call(left, method, rightExpression);
                    methodCallExpression = Expression.Not(methodCallExpression);
                }
                else
                {
                    rightExpression = Expression.Constant(searchTerm);
                    methodCallExpression = Expression.Call(left, method, rightExpression);
                }

                if (count == 0)
                {
                    searchExpression = methodCallExpression;
                }
                else
                {
                    string conditionOperator = operatorIndexes[count - 1].Operator.Trim();
                    switch (conditionOperator)
                    {
                        case "and":
                            searchExpression = Expression.AndAlso(searchExpression, methodCallExpression);
                            break;
                        case "or":
                            searchExpression = Expression.OrElse(searchExpression, methodCallExpression);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        Expression? finalExpression;
        Expression? nullOrEmptyCheck;
        if (addParentObjectNullCheck)
        {
            // Add Null check for nested Object before checking the value of the property.
            var nullCheck = Expression.NotEqual(Expression.PropertyOrField(pe, members[0]), Expression.Constant(null, typeof(object)));
            nullOrEmptyCheck = Expression.Not(Expression.Call(typeof(string), typeof(string).GetMethod("IsNullOrEmpty").Name, null, propertyExp));
            finalExpression = Expression.AndAlso(nullCheck, nullOrEmptyCheck);
            finalExpression = Expression.AndAlso(finalExpression, searchExpression);
        }
        else
        {
            nullOrEmptyCheck = Expression.Not(Expression.Call(typeof(string), typeof(string).GetMethod("IsNullOrEmpty").Name, null, propertyExp));
            finalExpression = Expression.AndAlso(nullOrEmptyCheck, searchExpression);
        }

        return finalExpression;
    }
}