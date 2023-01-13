using System;
using System.Linq.Expressions;

namespace Genocs.QueryBuilder
{
    /// <summary>
    /// The ExpressionBuilder for bool type
    /// </summary>
    public static partial class ExpressionBuilder
    {

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="searchTerms">The search terms.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="pe">The pe.</param>
        /// <returns></returns>
        internal static Expression GetExpressionBool<TSource>(string[] searchTerms, string propertyName, ParameterExpression pe)
        {
            // Compose the expression tree that represents the parameter to the predicate.
            Expression propertyExp = pe;
            foreach (var member in propertyName.Split('.'))
            {
                propertyExp = Expression.PropertyOrField(propertyExp, member);
            }

            Expression searchExpression = null;

            MethodCallExpression left = Expression.Call(propertyExp, typeof(bool).GetMethod("ToString", Type.EmptyTypes));
            left = Expression.Call(left, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            var searchTerm = searchTerms[0].ToLower();
            Expression rightExpression = Expression.Constant(searchTerm);
            searchExpression = Expression.Call(left, method, rightExpression);

            return searchExpression;
        }
    }
}
