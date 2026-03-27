using Genocs.Saga.Async;
using System.Reflection;
using Genocs.Saga.Builders;
using Genocs.Saga.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Saga;

public static class Extensions
{
    public static IServiceCollection AddSaga(this IServiceCollection services, Action<ISagaBuilder>? build = null)
        => AddSagaInternal(services, build, assemblies: null);

    public static IServiceCollection AddSaga(this IServiceCollection services, params Assembly[] assemblies)
        => AddSagaInternal(services, build: null, assemblies);

    public static IServiceCollection AddSaga(this IServiceCollection services, Action<ISagaBuilder> build, params Assembly[] assemblies)
        => AddSagaInternal(services, build, assemblies);

    private static IServiceCollection AddSagaInternal(IServiceCollection services, Action<ISagaBuilder>? build, Assembly[]? assemblies)
    {
        services.AddTransient<ISagaCoordinator, SagaCoordinator>();
        services.AddTransient<ISagaSeeker, SagaSeeker>();
        services.AddTransient<ISagaInitializer, SagaInitializer>();
        services.AddTransient<ISagaProcessor, SagaProcessor>();
        services.AddTransient<ISagaCompensationManager, SagaCompensationManager>();
        services.AddTransient<ISagaPostProcessor, SagaPostProcessor>();

        var sagaBuilder = new SagaBuilder(services);

        // Safe defaults first, then allow the callback to override registrations.
        sagaBuilder.UseInMemoryPersistence();
        sagaBuilder.UseInProcessExecutionLock();
        build?.Invoke(sagaBuilder);

        ValidatePersistenceRegistration(services);

        services.RegisterSagas(assemblies);

        return services;
    }

    private static void ValidatePersistenceRegistration(IServiceCollection services)
    {
        bool hasStateRepository = services.Any(sd => sd.ServiceType == typeof(ISagaStateRepository));
        bool hasSagaLog = services.Any(sd => sd.ServiceType == typeof(ISagaLog));
        bool hasExecutionLock = services.Any(sd => sd.ServiceType == typeof(ISagaExecutionLock));

        if (!hasStateRepository || !hasSagaLog || !hasExecutionLock)
        {
            throw new SagaException("Saga runtime is not fully configured. ISagaStateRepository, ISagaLog, and ISagaExecutionLock must be registered.");
        }
    }

    private static void RegisterSagas(this IServiceCollection services, Assembly[]? assemblies)
        => services.Scan(scan =>
        {
            IEnumerable<Assembly> discoveryAssemblies = GetDiscoveryAssemblies(assemblies);

            scan
                .FromAssemblies(discoveryAssemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(ISaga)))
                .As(t => t
                    .GetTypeInfo()
                    .GetInterfaces(includeInherited: false))
                .WithTransientLifetime();
        });

    private static IEnumerable<Assembly> GetDiscoveryAssemblies(Assembly[]? assemblies)
    {
        if (assemblies is not { Length: > 0 })
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        return assemblies
            .Where(static assembly => assembly is not null)
            .Distinct();
    }

    private static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited)
    {
        if (includeInherited || type.BaseType is null)
        {
            return type.GetInterfaces();
        }

        return type.GetInterfaces().Except(type.BaseType.GetInterfaces());
    }
}
