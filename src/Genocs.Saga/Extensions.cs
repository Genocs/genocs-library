using System.Reflection;
using Genocs.Saga.Builders;
using Genocs.Saga.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Saga;

public static class Extensions
{
    public static IServiceCollection AddSaga(this IServiceCollection services, Action<ISagaBuilder>? build = null)
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
        build?.Invoke(sagaBuilder);

        ValidatePersistenceRegistration(services);

        services.RegisterSagas();

        return services;
    }

    private static void ValidatePersistenceRegistration(IServiceCollection services)
    {
        bool hasStateRepository = services.Any(sd => sd.ServiceType == typeof(ISagaStateRepository));
        bool hasSagaLog = services.Any(sd => sd.ServiceType == typeof(ISagaLog));

        if (!hasStateRepository || !hasSagaLog)
        {
            throw new SagaException("Saga persistence is not fully configured. Both ISagaStateRepository and ISagaLog must be registered.");
        }
    }

    private static void RegisterSagas(this IServiceCollection services)
        => services.Scan(scan =>
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(ISaga)))
                .As(t => t
                    .GetTypeInfo()
                    .GetInterfaces(includeInherited: false))
                .WithTransientLifetime();
        });

    private static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited)
    {
        if (includeInherited || type.BaseType is null)
        {
            return type.GetInterfaces();
        }

        return type.GetInterfaces().Except(type.BaseType.GetInterfaces());
    }
}
