using Genocs.Common.CQRS.Commands;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands.Dispatchers;
using Genocs.Core.CQRS.Commons;
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
    /// <returns>The Genocs builder. You can use it for chain commands.</returns>
    public static IGenocsBuilder AddCommandHandlers(this IGenocsBuilder builder)
    {
        var assemblies = HandlerRegistration.GetCandidateAssemblies();
        builder.Services.AddHandlerRegistrations(assemblies, typeof(ICommandHandler<>), ServiceLifetime.Transient);

        return builder;
    }

    /// <summary>
    /// Add the In Memory command dispatcher to the DI container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The Genocs builder. You can use it for chain commands.</returns>
    public static IGenocsBuilder AddInMemoryCommandDispatcher(this IGenocsBuilder builder)
    {
        builder.Services.EmitDispatcherRegistrationDiagnostics(typeof(ICommandDispatcher), typeof(ICommandHandler<>));
        builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        return builder;
    }
}