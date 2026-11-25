using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.EFCore.Context;
using Genocs.Persistence.EFCore.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Repositories;

/// <summary>
/// Specification repository constructed with <typeparamref name="TContext"/>.
/// Updates and deletes of <see cref="IVersioned"/> aggregates use optimistic concurrency.
/// </summary>
/// <typeparam name="T">Aggregate root type.</typeparam>
/// <typeparam name="TContext">Caller DbContext.</typeparam>
public class GenocsRepository<T, TContext> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
    where T : class, IAggregateRoot
    where TContext : DbContext
{
    public GenocsRepository(TContext dbContext)
        : base(dbContext)
    {
        Context = dbContext;
    }

    internal TContext Context { get; }

    /// <summary>
    /// Persists pending changes. <see cref="IVersioned"/> aggregates increment <c>Version</c> only when the write is accepted.
    /// A stale version throws <see cref="OptimisticConcurrencyException"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => OptimisticConcurrency.SaveAsync(Context, token => base.SaveChangesAsync(token), cancellationToken);

    // Map to a DTO in the database only when the specification has no selector.
    protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
        => specification.Selector is not null
            ? base.ApplySpecification(specification)
            : ApplySpecification(specification, false)
                .ProjectToType<TResult>();
}

/// <summary>
/// Repository bound to <see cref="ApplicationDbContext"/>.
/// </summary>
/// <typeparam name="T">Type of entity.</typeparam>
public class ApplicationDbRepository<T> : GenocsRepository<T, ApplicationDbContext>
    where T : class, IAggregateRoot
{
    public ApplicationDbRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }
}
