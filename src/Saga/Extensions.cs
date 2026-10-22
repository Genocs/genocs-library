using Genocs.Saga.Async;
using System.Reflection;
using Genocs.Saga.Builders;
using Genocs.Saga.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

        SagaRegistrationDiagnostics diagnostics = services.RegisterSagas(assemblies);
        services.TryAddSingleton(diagnostics);
        services.TryAddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, SagaStartupDiagnosticsHostedService>());

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

    private static SagaRegistrationDiagnostics RegisterSagas(this IServiceCollection services, Assembly[]? assemblies)
    {
        Assembly[] discoveryAssemblies = [.. GetDiscoveryAssemblies(assemblies)];

        services.Scan(scan =>
        {
            IEnumerable<Assembly> assemblySource = discoveryAssemblies;

            scan
                .FromAssemblies(assemblySource)
                .AddClasses(classes => classes.AssignableTo(typeof(ISaga)))
                .As(t => t
                    .GetTypeInfo()
                    .GetInterfaces(includeInherited: false))
                .WithTransientLifetime();
        });

        return BuildDiagnostics(services, discoveryAssemblies);
    }

    private static IEnumerable<Assembly> GetDiscoveryAssemblies(Assembly[]? assemblies)
    {
        if (assemblies is not { Length: > 0 })
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(static assembly => !assembly.IsDynamic)
                .Distinct()
                .OrderBy(static assembly => assembly.GetName().Name, StringComparer.Ordinal);
        }

        return assemblies
            .Where(static assembly => assembly is not null && !assembly.IsDynamic)
            .Distinct()
            .OrderBy(static assembly => assembly.GetName().Name, StringComparer.Ordinal);
    }

    private static SagaRegistrationDiagnostics BuildDiagnostics(IServiceCollection services, IReadOnlyList<Assembly> discoveryAssemblies)
    {
        SagaTypeRegistrationDiagnostics[] sagaTypes = [..
            discoveryAssemblies
                .SelectMany(static assembly => assembly.DefinedTypes)
                .Where(static typeInfo =>
                    !typeInfo.IsAbstract &&
                    !typeInfo.IsInterface &&
                    (typeInfo.IsPublic || typeInfo.IsNestedPublic) &&
                    typeof(ISaga).IsAssignableFrom(typeInfo.AsType()))
                .Select(static typeInfo => BuildSagaTypeDiagnostics(typeInfo.AsType()))
                .OrderBy(static saga => saga.SagaType.FullName, StringComparer.Ordinal)];

        return new SagaRegistrationDiagnostics(
            discoveryAssemblies,
            sagaTypes,
            ResolveImplementationType(services, typeof(ISagaStateRepository)),
            ResolveImplementationType(services, typeof(ISagaLog)),
            ResolveImplementationType(services, typeof(ISagaExecutionLock)));
    }

    private static SagaTypeRegistrationDiagnostics BuildSagaTypeDiagnostics(Type sagaType)
    {
        SagaMessageRegistrationDiagnostics[] bindings = [..
            sagaType
                .GetInterfaces()
                .Where(static type => type.IsGenericType)
                .Select(static type => new
                {
                    InterfaceType = type.GetGenericTypeDefinition(),
                    MessageType = type.GetGenericArguments()[0]
                })
                .Where(static binding =>
                    binding.InterfaceType == typeof(ISagaAction<>) ||
                    binding.InterfaceType == typeof(ISagaStartAction<>))
                .GroupBy(static binding => binding.MessageType)
                .Select(static group => new SagaMessageRegistrationDiagnostics(
                    group.Key,
                    group.Any(static binding => binding.InterfaceType == typeof(ISagaStartAction<>))))
                .OrderBy(static binding => binding.MessageType.FullName, StringComparer.Ordinal)];

        return new SagaTypeRegistrationDiagnostics(sagaType, bindings);
    }

    private static Type ResolveImplementationType(IServiceCollection services, Type serviceType)
    {
        ServiceDescriptor descriptor = services.Last(sd => sd.ServiceType == serviceType);

        return descriptor.ImplementationType
            ?? descriptor.ImplementationInstance?.GetType()
            ?? serviceType;
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
