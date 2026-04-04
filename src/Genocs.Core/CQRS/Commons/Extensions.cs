using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Commons;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;
using Genocs.Common.Types;
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

        return services;
    }

    /// <summary>
    /// AddDispatchers Implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection. You can use it for chain commands.</returns>
    public static IServiceCollection AddDispatchers(this IServiceCollection services)
        => services
            .AddSingleton<IDispatcher, InMemoryDispatcher>()
            .AddSingleton<ICommandDispatcher, CommandDispatcher>()
            .AddSingleton<IEventDispatcher, EventDispatcher>()
            .AddSingleton<IQueryDispatcher, QueryDispatcher>();
}
