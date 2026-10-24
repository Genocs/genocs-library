namespace Genocs.Persistence.EFCore.MultiTenancy;

/// <summary>
/// Initializes the application database for a specific tenant.
/// Kept separate from <see cref="Persistence.Initialization.IDatabaseInitializer"/> so the core
/// initialization contract stays free of multitenancy types.
/// </summary>
public interface ITenantDatabaseInitializer
{
    /// <summary>
    /// Initializes (migrates and seeds) the application database for the given tenant,
    /// honoring a tenant-dedicated connection string when one is configured.
    /// </summary>
    /// <param name="tenant">The tenant to initialize the database for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task InitializeApplicationDbForTenantAsync(GNXTenantInfo tenant, CancellationToken cancellationToken);
}
