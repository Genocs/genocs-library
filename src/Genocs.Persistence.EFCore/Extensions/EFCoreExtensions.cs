using System.Reflection;
using Genocs.Common.Domain.Entities;
using Genocs.Common.Persistence;
using Genocs.Common.Persistence.Initialization;
using Genocs.Core.Builders;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.Context;
using Genocs.Persistence.EFCore.Initialization;
using Genocs.Persistence.EFCore.Persistence.Initialization;
using Genocs.Persistence.EFCore.Providers;
using Genocs.Persistence.EFCore.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Genocs.Persistence.EFCore.Extensions;

public static class EFCoreExtensions
{
    private static readonly ILogger _logger = Log.ForContext(typeof(EFCoreExtensions));

    public static IGenocsBuilder AddEFCorePersistence(this IGenocsBuilder builder, params Assembly[] additionalAssemblies)
    {
        // Bind the configuration section to the DatabaseOptions class
        // and validate it
        builder.Services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(nameof(DatabaseOptions))
            .PostConfigure(databaseSettings =>
            {
                _logger.Information("Current DB Provider: {dbProvider}", databaseSettings.DBProvider);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Add the DbContext and other services
        builder.Services
            .AddEFCoreDbProviders()
            .AddDbContext<ApplicationDbContext>((p, m) =>
            {
                var databaseSettings = p.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                p.GetServices<IEFCoreDbProvider>()
                    .Resolve(databaseSettings.DBProvider)
                    .Configure(m, databaseSettings);
            })
            .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
            .AddTransient<ApplicationDbInitializer>()
            .AddTransient<ApplicationDbSeeder>()
            .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
            .AddTransient<CustomSeederRunner>()
            .AddTransient<IDapperRepository, DapperRepository>()
            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
            .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()
            .AddRepositories(additionalAssemblies);

        return builder;
    }

    internal static IServiceCollection AddServices(this IServiceCollection services, Type interfaceType, ServiceLifetime lifetime)
    {
        var interfaceTypes =
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => interfaceType.IsAssignableFrom(t)
                            && t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterfaces().FirstOrDefault(),
                    Implementation = t
                })
                .Where(t => t.Service is not null
                            && interfaceType.IsAssignableFrom(t.Service));

        foreach (var type in interfaceTypes)
        {
            services.AddService(type.Service!, type.Implementation, lifetime);
        }

        return services;
    }

    internal static IServiceCollection AddService(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime) =>
        lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
            ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
            _ => throw new ArgumentException("Invalid lifeTime", nameof(lifetime))
        };

    internal static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Add Repositories
        services.AddScoped(typeof(IRepository<>), typeof(ApplicationDbRepository<>));

        // Always include the application's entry assembly, and let callers add more assemblies
        // for modular/feature-sliced aggregate roots that live outside the entry project.
        var entryAssembly = Assembly.GetEntryAssembly();
        var assembliesToScan = assemblies
            .Where(a => a is not null)
            .Append(entryAssembly)
            .OfType<Assembly>()
            .Distinct()
            .ToArray();

        foreach (var aggregateRootType in
            assembliesToScan
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
                .Distinct()
                .ToList())
        {
            // Add ReadRepositories.
            services.AddScoped(typeof(IReadRepository<>).MakeGenericType(aggregateRootType), sp =>
                sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)));

            // EventAddingRepositoryDecorator<T> carries a self-referential constraint (IAggregateRoot<T>)
            // that will be relaxed in EFCORE-017. Until then, only register IRepositoryWithEvents<T>
            // for types that actually satisfy the constraint to avoid an ArgumentException from MakeGenericType.
            var selfReferentialType = typeof(IAggregateRoot<>).MakeGenericType(aggregateRootType);
            if (selfReferentialType.IsAssignableFrom(aggregateRootType))
            {
                services.AddScoped(typeof(IRepositoryWithEvents<>).MakeGenericType(aggregateRootType), sp =>
                    Activator.CreateInstance(
                        typeof(EventAddingRepositoryDecorator<>).MakeGenericType(aggregateRootType),
                        sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)))
                    ?? throw new InvalidOperationException($"Couldn't create EventAddingRepositoryDecorator for aggregateRootType {aggregateRootType.Name}"));
            }
        }

        return services;
    }
}