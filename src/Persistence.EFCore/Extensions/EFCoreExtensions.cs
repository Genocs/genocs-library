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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        // Add the DbContext and other services.
        // The matching IEFCoreDbProvider must be registered by a Genocs.Persistence.EFCore.* provider package.
        builder.Services
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

    /// <summary>
    /// Registers <see cref="IRepository{T}"/> and <see cref="IReadRepository{T}"/> against
    /// <typeparamref name="TContext"/>, and registers <see cref="IRepositoryWithEvents{T}"/> for
    /// concrete aggregate roots in <paramref name="domainAssembly"/> that implement
    /// <see cref="IGeneratesDomainEvents"/>.
    /// </summary>
    /// <remarks>
    /// Scans only <paramref name="domainAssembly"/>. It does not scan <c>Genocs.Common</c>
    /// and it does not append <see cref="Assembly.GetEntryAssembly"/>.
    /// Open repository registrations are added only when those service types are not already
    /// registered, so a later call does not replace a context chosen by an earlier call.
    /// Use this method when the host DbContext is not <see cref="ApplicationDbContext"/>.
    /// <typeparamref name="TContext"/> must be public. The open repository type is generated
    /// in another assembly and cannot reference an internal context.
    /// </remarks>
    /// <typeparam name="TContext">Caller DbContext. A multitenant context is valid.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="domainAssembly">Assembly that contains the host aggregate roots.</param>
    public static IServiceCollection AddGenocsRepositories<TContext>(this IServiceCollection services, Assembly domainAssembly)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(domainAssembly);

        AddGenocsRepositoryServices<TContext>(services);
        AddAggregateEventRepositories(services, domainAssembly);
        return services;
    }

    internal static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] assemblies)
    {
        AddGenocsRepositoryServices<ApplicationDbContext>(services);

        // Always include the application's entry assembly, and let callers add more assemblies
        // for modular/feature-sliced aggregate roots that live outside the entry project.
        var entryAssembly = Assembly.GetEntryAssembly();
        var assembliesToScan = assemblies
            .Where(a => a is not null)
            .Append(entryAssembly)
            .OfType<Assembly>()
            .Distinct()
            .ToArray();

        foreach (var assembly in assembliesToScan)
        {
            AddAggregateEventRepositories(services, assembly);
        }

        return services;
    }

    private static void AddGenocsRepositoryServices<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var repositoryType = OpenGenericRepositoryType.For(typeof(TContext));
        services.TryAddScoped(typeof(IRepository<>), repositoryType);
        services.TryAddScoped(typeof(IReadRepository<>), repositoryType);
    }

    private static void AddAggregateEventRepositories(IServiceCollection services, Assembly assembly)
    {
        foreach (var aggregateRootType in assembly
            .GetExportedTypes()
            .Where(t => t.IsClass
                        && !t.IsAbstract
                        && !t.IsGenericTypeDefinition
                        && typeof(IAggregateRoot).IsAssignableFrom(t)
                        && typeof(IGeneratesDomainEvents).IsAssignableFrom(t)))
        {
            var serviceType = typeof(IRepositoryWithEvents<>).MakeGenericType(aggregateRootType);
            if (services.Any(d => d.ServiceType == serviceType))
            {
                continue;
            }

            var repositoryType = typeof(IRepository<>).MakeGenericType(aggregateRootType);
            var decoratorType = typeof(EventAddingRepositoryDecorator<>).MakeGenericType(aggregateRootType);
            services.AddScoped(serviceType, sp =>
            {
                object repository = sp.GetRequiredService(repositoryType);
                return ActivatorUtilities.CreateInstance(sp, decoratorType, repository);
            });
        }
    }
}