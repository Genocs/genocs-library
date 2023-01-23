using System;
using System.Linq.Expressions;

namespace Genocs.QueryBuilder
{
    public static partial class ExpressionBuilder
    {
        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="searchTerms">The search terms.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="operatorType">The operator type.</param>
        /// <param name="pe">The parameter expression.</param>
        /// <returns></returns>
        internal static Expression GetExpressionDate<TSource>(string[] searchTerms, string propertyName, QueryOperator operatorType, ParameterExpression pe)
        {
            // Compose the expression tree that represents the parameter to the predicate.
            Expression propertyExp = pe;
            foreach (var member in propertyName.Split('.'))
            {
                propertyExp = Expression.PropertyOrField(propertyExp, member);
            }

            // To do for date management
            MethodCallExpression left = Expression.Call(propertyExp, typeof(DateTime).GetMethod("ToString", new Type[] { typeof(String) }), Expression.Constant(Constants.DATE_FORMAT));
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            ConstantExpression constantExpression = Expression.Constant(DateTime.Parse(searchTerms[0].ToLower()));

            var nullCheck = NormalizeNullable(propertyExp, constantExpression);


            Expression? searchExpression;
            switch (operatorType)
            {
                case QueryOperator.GreaterThan:
                    searchExpression = Expression.GreaterThan(nullCheck.Item1, nullCheck.Item2);
                    break;
                case QueryOperator.GreaterThanOrEqual:
                    searchExpression = Expression.GreaterThanOrEqual(nullCheck.Item1, nullCheck.Item2);
                    break;
                case QueryOperator.LessThan:
                    searchExpression = Expression.LessThan(nullCheck.Item1, nullCheck.Item2);
                    break;
                case QueryOperator.LessThanOrEqual:
                    searchExpression = Expression.LessThanOrEqual(nullCheck.Item1, nullCheck.Item2);
                    break;
                case QueryOperator.NotEqual:
                    searchExpression = Expression.NotEqual(nullCheck.Item1, nullCheck.Item2);
                    break;
                case QueryOperator.Equal:
                default:
                    searchExpression = Expression.Equal(nullCheck.Item1, nullCheck.Item2);
                    break;
            }

            return searchExpression;
        }
    }
}