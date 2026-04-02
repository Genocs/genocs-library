using Genocs.Persistence.EFCore.MultiTenancy;

namespace Genocs.Library.Demo.WebApi.Extensions;

public static class MultiTenancyExtensions
{
    public static IServiceCollection AddDemoFinbuckleMultiTenancy(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Finbuckle multitenancy with configuration store (host strategy)
        services.AddFinbuckleMultiTenancy<GNXTenantInfo>(configuration);

        // Optionally, register EF Core store for tenants
        // services.AddFinbuckleMultiTenancyWithEfCoreStore<TenantInfo, BookStoreDbContext>();

        return services;
    }
}
