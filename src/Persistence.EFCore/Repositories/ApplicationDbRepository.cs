using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.EFCore.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Repositories;

/// <summary>
/// Specification repository constructed with <typeparamref name="TContext"/>.
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
