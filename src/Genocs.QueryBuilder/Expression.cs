using System.Linq.Expressions;

namespace Genocs.QueryBuilder;

public static partial class ExpressionBuilder
{

    #region Private static members
    private static readonly string NotOperator = " not ";
    #endregion

    private static Tuple<Expression, Expression> NormalizeNullable(Expression e1, Expression e2)
    {
        if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
            e2 = Expression.Convert(e2, e1.Type);
        else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
            e1 = Expression.Convert(e1, e2.Type);
        return new Tuple<Expression, Expression>(e1, e2);
    }

    private static bool IsNullableType(Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}