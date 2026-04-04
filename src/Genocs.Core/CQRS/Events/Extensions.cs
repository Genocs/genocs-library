using Genocs.Common.CQRS.Events;
using Genocs.Common.Types;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commons;
using Genocs.Core.CQRS.Events.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.CQRS.Events;

/// <summary>
/// CQRS events extensions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// AddEventHandlers.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The updated Genocs builder.</returns>
    public static IGenocsBuilder AddEventHandlers(this IGenocsBuilder builder)
    {
        var assemblies = HandlerRegistration.GetCandidateAssemblies();
        builder.Services.AddHandlerRegistrations(assemblies, typeof(IEventHandler<>), ServiceLifetime.Transient);

        return builder;
    }

    /// <summary>
    /// AddInMemoryEventDispatcher.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The updated Genocs builder.</returns>
    public static IGenocsBuilder AddInMemoryEventDispatcher(this IGenocsBuilder builder)
    {
        builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
        return builder;
    }

    /// <summary>
    /// AddEventHandlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        var assemblies = HandlerRegistration.GetCandidateAssemblies();
        services.AddHandlerRegistrations(assemblies, typeof(IEventHandler<>), ServiceLifetime.Transient);

        return services;
    }

    /// <summary>
    /// AddInMemoryEventDispatcher.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInMemoryEventDispatcher(this IServiceCollection services)
    {
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        return services;
    }
}