using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Context;

/// <summary>
/// Model conventions used by Genocs EF Core persistence.
/// </summary>
public static class GenocsModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Registers Genocs conventions, including optimistic concurrency for aggregates that implement
    /// <c>IVersioned</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="ApplicationDbContext"/> registers these conventions automatically.
    /// A caller <see cref="DbContext"/> passed to <c>AddGenocsRepositories</c> must call this method
    /// from <c>ConfigureConventions</c>. Without it, saving an <c>IVersioned</c> aggregate throws
    /// <see cref="InvalidOperationException"/> instead of silently dropping the version check.
    /// </remarks>
    /// <param name="configurationBuilder">Convention builder from <c>DbContext.ConfigureConventions</c>.</param>
    /// <returns>The same builder, so calls can be chained.</returns>
    public static ModelConfigurationBuilder AddGenocsConventions(this ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        configurationBuilder.Conventions.Add(_ => new VersionedOptimisticConcurrencyConvention());
        return configurationBuilder;
    }
}
