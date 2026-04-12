using System;
using System.Linq;
using System.Reflection;
using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Core.Builders;
using Genocs.Logging.CQRS.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Logging.CQRS;

public static class Extensions
{
    public static IGenocsBuilder AddCommandHandlersLogging(this IGenocsBuilder builder, Assembly assembly = null)
        => builder.AddHandlerLogging(typeof(ICommandHandler<>), typeof(CommandHandlerLoggingDecorator<>), assembly);

    public static IGenocsBuilder AddEventHandlersLogging(this IGenocsBuilder builder, Assembly assembly = null)
        => builder.AddHandlerLogging(typeof(IEventHandler<>), typeof(EventHandlerLoggingDecorator<>), assembly);

    private static IGenocsBuilder AddHandlerLogging(this IGenocsBuilder builder, Type handlerType, Type decoratorType, Assembly assembly = null)
    {
        assembly ??= ResolveDefaultAssembly();

        var handlerContracts = assembly
            .DefinedTypes
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.ImplementedInterfaces)
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType)
            .Distinct()
            .ToList();

        foreach (var handlerContract in handlerContracts)
        {
            var messageType = handlerContract.GenericTypeArguments.SingleOrDefault();
            if (messageType is null)
            {
                continue;
            }

            var closedDecoratorType = decoratorType.MakeGenericType(messageType);
            builder.Services.TryDecorate(handlerContract, closedDecoratorType);
        }

        return builder;
    }

    private static Assembly ResolveDefaultAssembly()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly?.IsDynamic == false)
        {
            return entryAssembly;
        }

        return Assembly.GetCallingAssembly();
    }
}