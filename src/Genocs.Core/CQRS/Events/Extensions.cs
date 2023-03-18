namespace Genocs.Core.CQRS.Events;

using Genocs.Core.Builders;
using Genocs.Core.CQRS.Events.Dispatchers;
using Genocs.Core.Types;
using Microsoft.Extensions.DependencyInjection;
using System;

/// <summary>
/// CQRS events extensions
/// </summary>
public static class Extensions
{
    /// <summary>
    /// AddEventHandlers
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IGenocsBuilder AddEventHandlers(this IGenocsBuilder builder)
    {
        builder.Services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>))
                    .WithoutAttribute(typeof(DecoratorAttribute)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        return builder;
    }


    /// <summary>
    /// AddInMemoryEventDispatcher
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IGenocsBuilder AddInMemoryEventDispatcher(this IGenocsBuilder builder)
    {
        builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
        return builder;
    }

    /// <summary>
    /// AddEventHandlers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>))
                    .WithoutAttribute(typeof(DecoratorAttribute)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        return services;
    }

    /// <summary>
    /// AddInMemoryEventDispatcher
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddInMemoryEventDispatcher(this IServiceCollection services)
    {
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        return services;
    }
}