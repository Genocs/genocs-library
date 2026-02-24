using System.Reflection;
using System.Runtime.CompilerServices;
using Genocs.Common.Cqrs.Commands;
using Genocs.Common.Cqrs.Events;
using Genocs.Core.Builders;
using Genocs.Logging.Cqrs.Decorators;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Genocs.Logging.Cqrs;

public static class Extensions
{
    public static IGenocsBuilder AddCommandHandlersLogging(this IGenocsBuilder builder, Assembly? assembly = null)
        => builder.AddHandlerLogging(typeof(ICommandHandler<>), typeof(CommandHandlerLoggingDecorator<>), assembly);

    public static IGenocsBuilder AddEventHandlersLogging(this IGenocsBuilder builder, Assembly? assembly = null)
        => builder.AddHandlerLogging(typeof(IEventHandler<>), typeof(EventHandlerLoggingDecorator<>), assembly);

    private static IGenocsBuilder AddHandlerLogging(this IGenocsBuilder builder, Type handlerType, Type decoratorType, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var handlers = assembly
            .GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType))
            .ToList();

        handlers.ForEach(ch => GetExtensionMethods()
            .FirstOrDefault(mi => !mi.IsGenericMethod && mi.Name == "TryDecorate")?
            .Invoke(builder.Services,
            [
                builder.Services,
                ch.GetInterfaces().FirstOrDefault(),
                decoratorType.MakeGenericType(ch.GetInterfaces().FirstOrDefault()?.GenericTypeArguments.First())
            ]));

        return builder;
    }

    private static IEnumerable<MethodInfo> GetExtensionMethods()
    {
        var types = typeof(ReplacementBehavior).Assembly.GetTypes();

        var query = from type in types
                    where type.IsSealed && !type.IsGenericType && !type.IsNested
                    from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    where method.IsDefined(typeof(ExtensionAttribute), false)
                    where method.GetParameters()[0].ParameterType == typeof(IServiceCollection)
                    select method;

        return query;
    }
}