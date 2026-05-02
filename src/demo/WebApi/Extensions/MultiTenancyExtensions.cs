using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.MultiTenancy;

namespace Genocs.Library.Demo.WebApi.Extensions;

/// <summary>
/// Extension method to register Finbuckle multi-tenancy services in the dependency injection container.
/// </summary>
public static class MultiTenancyExtensions
{
    /// <summary>
    /// Adds Finbuckle multi-tenancy services to the dependency injection container.
    /// TODO: Work in progress. Evalaute to use IGenocsBuilder instead of IServiceCollection and adjust the method signature accordingly.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The IConfiguration instance to read tenant configuration from.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddApplicationMultiTenancy(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Finbuckle multitenancy with configuration store (host strategy)
        services.AddFinbuckleMultiTenancy<GNXTenantInfo>(configuration);

        // Optionally, register EF Core store for tenants
        // services.AddFinbuckleMultiTenancyWithEfCoreStore<TenantInfo, BookStoreDbContext>();

        return services;
    }

    public static IGenocsBuilder AddApplicationMultiTenancy(this IGenocsBuilder builder, IConfiguration? configuration = null)
    {
        if (configuration == null && builder.Configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null. Please provide a valid IConfiguration instance.");
        }

        // Register Finbuckle multitenancy with configuration store (host strategy)
        builder.Services.AddFinbuckleMultiTenancy<GNXTenantInfo>(builder.Configuration!);

        // Optionally, register EF Core store for tenants
        // services.AddFinbuckleMultiTenancyWithEfCoreStore<TenantInfo, BookStoreDbContext>();
        return builder;
    }

    public static void UseMultiTenancy(this WebApplication app)
        => app.UseMultiTenancy();

}