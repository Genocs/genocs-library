using Genocs.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Genocs.Persistence.EFCore.Context;

/// <summary>
/// Marks <see cref="IVersioned.Version"/> as an EF Core concurrency token.
/// </summary>
internal sealed class VersionedOptimisticConcurrencyConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!typeof(IVersioned).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            entityType.FindProperty(nameof(IVersioned.Version))
                ?.Builder
                .IsConcurrencyToken(true);
        }
    }
}
