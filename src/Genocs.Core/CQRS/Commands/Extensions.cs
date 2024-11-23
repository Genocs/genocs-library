using Genocs.Common.Types;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.CQRS.Commands;

/// <summary>
/// Extension methods.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Add all the Command handlers to the DI container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder to be used for chaining pattern.</returns>
    public static IGenocsBuilder AddCommandHandlers(this IGenocsBuilder builder)
    {
        builder.Services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>))
                    .WithoutAttribute(typeof(DecoratorAttribute)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        return builder;
    }

    /// <summary>
    /// Add the In Memory command dispatcher to the DI container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder to be used for chaining pattern.</returns>
    public static IGenocsBuilder AddInMemoryCommandDispatcher(this IGenocsBuilder builder)
    {
        builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        return builder;
    }
}