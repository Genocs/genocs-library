using Genocs.Common.Domain.Entities;
using Genocs.Persistence.EFCore.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Context;

/// <summary>
/// Assigns the next <see cref="IVersioned.Version"/> and translates EF concurrency failures.
/// The version stored on the aggregate is the caller's expected token. It increases only when the write is accepted.
/// </summary>
internal static class OptimisticConcurrency
{
    public static int Save(DbContext context, Func<int> save)
    {
        Prepare(context);
        try
        {
            return save();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var concurrencyException = OptimisticConcurrencyException.From(exception);
            Revert(context);
            throw concurrencyException;
        }
    }

    public static async Task<int> SaveAsync(
        DbContext context,
        Func<CancellationToken, Task<int>> save,
        CancellationToken cancellationToken)
    {
        Prepare(context);
        try
        {
            return await save(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var concurrencyException = OptimisticConcurrencyException.From(exception);
            Revert(context);
            throw concurrencyException;
        }
    }

    private static void Prepare(DbContext context)
    {
        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries<IVersioned>())
        {
            if (entry.State is not (EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            var version = entry.Property(nameof(IVersioned.Version));
            if (!version.Metadata.IsConcurrencyToken)
            {
                throw new InvalidOperationException(
                    $"'{entry.Metadata.ClrType.Name}' implements '{nameof(IVersioned)}', but '{nameof(IVersioned.Version)}' is not an EF Core concurrency token. " +
                    $"Call '{nameof(GenocsModelConfigurationBuilderExtensions.AddGenocsConventions)}' from DbContext.ConfigureConventions. " +
                    $"{nameof(ApplicationDbContext)} already does this.");
            }

            // Deletes keep the loaded version so the DELETE predicate can reject a stale row.
            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            if (version.OriginalValue is not long expected)
            {
                throw new InvalidOperationException(
                    $"'{entry.Metadata.ClrType.Name}.{nameof(IVersioned.Version)}' must be a long optimistic concurrency token.");
            }

            version.CurrentValue = expected + 1;
        }
    }

    private static void Revert(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<IVersioned>())
        {
            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            var version = entry.Property(nameof(IVersioned.Version));
            if (version.OriginalValue is long expected)
            {
                version.CurrentValue = expected;
            }
        }
    }
}
