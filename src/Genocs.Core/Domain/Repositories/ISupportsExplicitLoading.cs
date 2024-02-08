using Genocs.Core.Domain.Entities;
using System.Linq.Expressions;

namespace Genocs.Core.Domain.Repositories;

public interface ISupportsExplicitLoading<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    Task EnsureCollectionLoadedAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> collectionExpression,
        CancellationToken cancellationToken)
        where TProperty : class;

    Task EnsurePropertyLoadedAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression,
        CancellationToken cancellationToken)
        where TProperty : class;
}