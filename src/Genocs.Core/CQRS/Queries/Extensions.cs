using Genocs.Common.Types;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Queries.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.CQRS.Queries;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    /// <summary>
    /// AddQueryHandlers
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IGenocsBuilder AddQueryHandlers(this IGenocsBuilder builder)
    {
        builder.Services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))
                    .WithoutAttribute(typeof(DecoratorAttribute)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        return builder;
    }

    /// <summary>
    /// AddInMemoryQueryDispatcher
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IGenocsBuilder AddInMemoryQueryDispatcher(this IGenocsBuilder builder)
    {
        builder.Services.AddSingleton<IQueryDispatcher, QueryDispatcher>();

        return builder;
    }
}