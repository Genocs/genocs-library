using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Genocs.Persistence.EFCore.Context;
using Genocs.Persistence.EFCore.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genocs.Persistence.EFCore.MultiTenancy;

internal class TenantDatabaseInitializer : ITenantDatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TenantDatabaseInitializer> _logger;

    public TenantDatabaseInitializer(IServiceProvider serviceProvider, ILogger<TenantDatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeApplicationDbForTenantAsync(GNXTenantInfo tenant, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        _logger.LogInformation("Initializing application database for tenant '{TenantId}'.", tenant.Id);

        // A dedicated scope isolates this tenant's initialization from the ambient request context.
        using var scope = _serviceProvider.CreateScope();

        // Inject the tenant into the scope via Finbuckle's setter so that all scoped services
        // resolved within this scope (e.g. ApplicationDbSeeder) can read the current tenant's
        // details through IMultiTenantContext<GNXTenantInfo>.
        var contextSetter = scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
        contextSetter.MultiTenantContext = new MultiTenantContext<GNXTenantInfo> { TenantInfo = tenant };

        // When the tenant owns a dedicated database, redirect the scoped ApplicationDbContext to
        // that connection string before the ApplicationDbInitializer captures the context via DI.
        // For tenants sharing the root database the connection string is empty and no override is needed.
        if (!string.IsNullOrWhiteSpace(tenant.ConnectionString))
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.SetConnectionString(tenant.ConnectionString);
        }

        await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
            .InitializeAsync(cancellationToken);

        _logger.LogInformation("Database initialization completed for tenant '{TenantId}'.", tenant.Id);
    }
}
