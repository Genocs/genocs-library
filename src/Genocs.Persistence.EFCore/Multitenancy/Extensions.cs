using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Genocs.Persistence.EFCore.MultiTenancy;

/// <summary>
/// Finbuckle multitenancy integration helpers for Genocs.
/// </summary>
public static class Extensions
{
    private const string DefaultConfigurationStoreSection = "Finbuckle:MultiTenant:Stores:ConfigurationStore";

    /// <summary>
    /// Adds Finbuckle multitenancy using host-based strategy and configuration store.
    /// </summary>
    /// <typeparam name="TTenantInfo">Tenant info type.</typeparam>
    /// <param name="builder">Genocs builder.</param>
    /// <param name="configuration">Configuration root used to resolve tenant definitions.</param>
    /// <param name="configurationStoreSection">Configuration section containing tenant entries.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IGenocsBuilder AddFinbuckleMultiTenancy<TTenantInfo>(
        this IGenocsBuilder builder,
        IConfiguration configuration,
        string configurationStoreSection = DefaultConfigurationStoreSection)
        where TTenantInfo : class, ITenantInfo, new()
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        builder.Services.AddFinbuckleMultiTenancy<TTenantInfo>(configuration, configurationStoreSection);
        return builder;
    }

    /// <summary>
    /// Adds Finbuckle multitenancy using host-based strategy and configuration store.
    /// </summary>
    /// <typeparam name="TTenantInfo">Tenant info type.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration root used to resolve tenant definitions.</param>
    /// <param name="configurationStoreSection">Configuration section containing tenant entries.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddFinbuckleMultiTenancy<TTenantInfo>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationStoreSection = DefaultConfigurationStoreSection)
        where TTenantInfo : class, ITenantInfo, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddMultiTenant<TTenantInfo>()
            .WithHostStrategy()
            .WithConfigurationStore(configuration, configurationStoreSection);

        return services;
    }

    /// <summary>
    /// Adds Finbuckle multitenancy using an EF Core tenant store for <see cref="GNXTenantInfo"/>.
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IGenocsBuilder AddFinbuckleMultiTenancyWithEfCoreStore(this IGenocsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddFinbuckleMultiTenancyWithEfCoreStore();
        return builder;
    }

    /// <summary>
    /// Adds Finbuckle multitenancy using an EF Core tenant store for <see cref="GNXTenantInfo"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddFinbuckleMultiTenancyWithEfCoreStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddEFCoreDbProviders()
            .AddDbContext<TenantDbContext>((p, m) =>
            {
                // TODO: We should probably add specific dbprovider/connectionstring setting for the tenantDb with a fallback to the main databasesettings
                var databaseSettings = p.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                p.GetServices<IEFCoreDbProvider>()
                    .Resolve(databaseSettings.DBProvider)
                    .Configure(m, databaseSettings);
            })
            .AddMultiTenant<GNXTenantInfo>()
                .WithClaimStrategy(GNXClaims.Tenant)
                .WithHeaderStrategy(MultitenancyConstants.TenantIdName)
                .WithQueryStringStrategy(MultitenancyConstants.TenantIdName)
                .WithEFCoreStore<TenantDbContext, GNXTenantInfo>() // Use EF Core store. Keep in mind only one store can be used at a time.
                .Services
            .AddScoped<ITenantService, TenantService>();
    }

    /// <summary>
    /// Adds Finbuckle multitenancy and configures an EF Core-backed tenant store.
    /// </summary>
    /// <typeparam name="TTenantInfo">Tenant info type.</typeparam>
    /// <typeparam name="TStoreDbContext">DbContext used by Finbuckle EF Core store.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional extra builder configuration, e.g. adding more strategies.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddFinbuckleMultiTenancyWithEfCoreStore<TTenantInfo, TStoreDbContext>(
        this IServiceCollection services,
        Action<MultiTenantBuilder<TTenantInfo>>? configure = null)
        where TTenantInfo : class, ITenantInfo, new()
        where TStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
    {
        ArgumentNullException.ThrowIfNull(services);

        var multitenancyBuilder = services.AddMultiTenant<TTenantInfo>()
            .WithEFCoreStore<TStoreDbContext, TTenantInfo>();

        configure?.Invoke(multitenancyBuilder);

        return services;
    }

    private static MultiTenantBuilder<GNXTenantInfo> WithQueryStringStrategy(this MultiTenantBuilder<GNXTenantInfo> builder, string queryStringKey)
        => builder.WithDelegateStrategy(context =>
        {
            if (context is not HttpContext httpContext)
            {
                return Task.FromResult((string?)null);
            }

            httpContext.Request.Query.TryGetValue(queryStringKey, out var tenantIdParam);

            return Task.FromResult((string?)tenantIdParam.ToString());
        });

    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
        => app.UseMultiTenant();
}
