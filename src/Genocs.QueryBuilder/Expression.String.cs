using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Genocs.QueryBuilder
{
    /// <summary>
    /// ExpressionBuilder class
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
        /// <param name="pe">The pe.</param>
        /// <param name="addParentObjectNullCheck">if set to <c>true</c> [add parent object null check].</param>
        /// <returns></returns>
        internal static Expression GetExpressionString<TSource>(string[] searchTerms, 
                                                        string keyword,
                                                        string propertyName,
                                                        List<OperatorIndexes> operatorIndexes,
                                                        ParameterExpression pe,
                                                        bool addParentObjectNullCheck)
        {
            // Compose the expression tree that represents the parameter to the predicate.
            Expression propertyExp = pe;
            string[] members = propertyName.Split('.');
            foreach (var member in members)
            {
                propertyExp = Expression.PropertyOrField(propertyExp, member);
            }

            Expression? searchExpression = null;
            Expression? finalExpression = null;
            Expression? nullorEmptyCheck = null;

            MethodCallExpression left = Expression.Call(propertyExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            for (int count = 0; count < searchTerms.Length; count++)
            {
                var searchTerm = searchTerms[count].ToLower();
                searchTerm = searchTerm.Replace("*", string.Empty);
                searchTerm = searchTerm.Replace("\"", string.Empty);
                Expression? methodCallExpresssion = null;
                Expression? rightExpression = null;

                if (searchTerm.Contains(NotOperator.TrimStart()))
                {
                    searchTerm = searchTerm.Replace(NotOperator.TrimStart(), string.Empty).Trim();
                    rightExpression = Expression.Constant(searchTerm);
                    methodCallExpresssion = Expression.Call(left, method, rightExpression);
                    methodCallExpresssion = Expression.Not(methodCallExpresssion);
                }
                else
                {
                    rightExpression = Expression.Constant(searchTerm);
                    methodCallExpresssion = Expression.Call(left, method, rightExpression);
                }

                if (count == 0)
                {
                    searchExpression = methodCallExpresssion;
                }
                else
                {
                    var conditionOperator = operatorIndexes[count - 1].Operator.Trim();
                    switch (conditionOperator)
                    {
                        case "and":
                            searchExpression = Expression.AndAlso(searchExpression, methodCallExpresssion);
                            break;
                        case "or":
                            searchExpression = Expression.OrElse(searchExpression, methodCallExpresssion);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (addParentObjectNullCheck)
            {
                //Add Null check for nested Object before checking the value of the property.
                var nullCheck = Expression.NotEqual(Expression.PropertyOrField(pe, members[0]), Expression.Constant(null, typeof(object)));
                nullorEmptyCheck = Expression.Not(Expression.Call(typeof(string), typeof(string).GetMethod("IsNullOrEmpty").Name, null, propertyExp));
                finalExpression = Expression.AndAlso(nullCheck, nullorEmptyCheck);
                finalExpression = Expression.AndAlso(finalExpression, searchExpression);
            }
            else
            {
                nullorEmptyCheck = Expression.Not(Expression.Call(typeof(string), typeof(string).GetMethod("IsNullOrEmpty").Name, null, propertyExp));
                finalExpression = Expression.AndAlso(nullorEmptyCheck, searchExpression);
            }

            return finalExpression;
        }
    }
}