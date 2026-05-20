using System.Linq.Expressions;

namespace Genocs.Common.Domain.Specifications;

/// <summary>
/// Provider-agnostic query specification contract.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public interface ISpecification<TEntity>
{
    /// <summary>
    /// Gets the optional filtering predicate.
    /// </summary>
    Expression<Func<TEntity, bool>> Criteria { get; }

    /// <summary>
    /// Gets strongly-typed include expressions when supported by the provider.
    /// </summary>
    IReadOnlyCollection<Expression<Func<TEntity, object>>> Includes { get; }

    /// <summary>
    /// Gets string-based include paths when expression includes are not sufficient.
    /// </summary>
    IReadOnlyCollection<string> IncludeStrings { get; }

    /// <summary>
    /// Gets ordering expressions applied in ascending order.
    /// </summary>
    IReadOnlyCollection<Expression<Func<TEntity, object>>> OrderByExpressions { get; }

    /// <summary>
    /// Gets ordering expressions applied in descending order.
    /// </summary>
    IReadOnlyCollection<Expression<Func<TEntity, object>>> OrderByDescendingExpressions { get; }

    /// <summary>
    /// Gets the optional number of records to skip.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets the optional number of records to take.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets a value indicating whether providers should execute read-only/no-tracking queries when possible.
    /// </summary>
    bool AsNoTracking { get; }
}

/// <summary>
/// Projection-capable specification contract.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TResult">Projection result type.</typeparam>
public interface IProjectionSpecification<TEntity, TResult> : ISpecification<TEntity>
{
    /// <summary>
    /// Gets the projection expression.
    /// </summary>
    Expression<Func<TEntity, TResult>> Selector { get; }
}
