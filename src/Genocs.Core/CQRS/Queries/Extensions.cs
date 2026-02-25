using Genocs.Common.CQRS.Queries;
using Genocs.Common.Types;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Queries.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.CQRS.Queries;

/// <summary>
/// Extension helper class for CQRS Queries.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// AddQueryHandlers.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The updated Genocs builder.</returns>
    public static IGenocsBuilder AddQueryHandlers(this IGenocsBuilder builder)
    {
        builder.Services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))
                    .WithoutAttribute<DecoratorAttribute>())
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        return builder;
    }

    /// <summary>
    /// AddInMemoryQueryDispatcher.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The updated Genocs builder.</returns>
    public static IGenocsBuilder AddInMemoryQueryDispatcher(this IGenocsBuilder builder)
    {
        builder.Services.AddSingleton<IQueryDispatcher, QueryDispatcher>();

        return builder;
    }
}