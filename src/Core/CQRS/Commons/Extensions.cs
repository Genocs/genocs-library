using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Commons;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands.Dispatchers;
using Genocs.Core.CQRS.Events.Dispatchers;
using Genocs.Core.CQRS.Queries.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.CQRS.Commons;

/// <summary>
/// Extension helper to handle the whole set of Dispatcher.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// AddHandlers implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="project">Name of the project.</param>
    /// <returns>The service collection. You can use it for chain commands.</returns>
    public static IServiceCollection AddHandlers(this IServiceCollection services, string project)
    {
        var assemblies = HandlerRegistration.GetCandidateAssemblies(project);

        services
            .AddHandlerRegistrations(assemblies, typeof(ICommandHandler<>), ServiceLifetime.Transient)
            .AddHandlerRegistrations(assemblies, typeof(IEventHandler<>), ServiceLifetime.Transient)
            .AddHandlerRegistrations(assemblies, typeof(IQueryHandler<,>), ServiceLifetime.Transient);

        CoreDiagnosticsRuntime.Info(services, $"Scanned {assemblies.Length} assembly(ies) for handlers using project filter '{project}'.");

        return services;
    }

    /// <summary>
    /// AddDispatchers Implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection. You can use it for chain commands.</returns>
    public static IServiceCollection AddDispatchers(this IServiceCollection services)
    {
        if (ShouldWarnOnEmptyHandlers(services, typeof(ICommandHandler<>)))
        {
            CoreDiagnosticsRuntime.Warn(services, "ICommandDispatcher registered with no command handlers discovered.");
        }

        if (ShouldWarnOnEmptyHandlers(services, typeof(IEventHandler<>)))
        {
            CoreDiagnosticsRuntime.Warn(services, "IEventDispatcher registered with no event handlers discovered.");
        }

        if (ShouldWarnOnEmptyHandlers(services, typeof(IQueryHandler<,>)))
        {
            CoreDiagnosticsRuntime.Warn(services, "IQueryDispatcher registered with no query handlers discovered.");
        }

        return services
            .AddSingleton<IDispatcher, InMemoryDispatcher>()
            .AddSingleton<ICommandDispatcher, CommandDispatcher>()
            .AddSingleton<IEventDispatcher, EventDispatcher>()
            .AddSingleton<IQueryDispatcher, QueryDispatcher>();
    }

    internal static void EmitDispatcherRegistrationDiagnostics(
        this IServiceCollection services,
        Type dispatcherInterfaceType,
        Type handlerInterfaceType)
    {
        if (ShouldWarnOnEmptyHandlers(services, handlerInterfaceType))
        {
            CoreDiagnosticsRuntime.Warn(
                services,
                $"{dispatcherInterfaceType.Name} registered with no {handlerInterfaceType.Name} handlers discovered.");
            return;
        }

        CoreDiagnosticsRuntime.Info(
            services,
            $"{dispatcherInterfaceType.Name} registered after at least one {handlerInterfaceType.Name} handler registration.");
    }

    private static bool ShouldWarnOnEmptyHandlers(IServiceCollection services, Type handlerInterfaceType)
    {
        if (!CoreDiagnosticsRuntime.TryGetEnabledState(services, out CoreDiagnosticsOptions? options, out _)
            || options?.WarnOnEmptyHandlerSet != true)
        {
            return false;
        }

        return !HasRegisteredHandler(services, handlerInterfaceType);
    }

    private static bool HasRegisteredHandler(IServiceCollection services, Type handlerInterfaceType)
    {
        return services.Any(descriptor =>
            descriptor.ServiceType.IsGenericType
            && descriptor.ServiceType.GetGenericTypeDefinition() == handlerInterfaceType);
    }
}
