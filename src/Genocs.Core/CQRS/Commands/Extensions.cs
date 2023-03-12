namespace Genocs.Core.CQRS.Commands
{
    using Genocs.Core.Builders;
    using Genocs.Core.CQRS.Commands.Dispatchers;
    using Genocs.Core.Types;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Extension method
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// AddCommandHandlers
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
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
        /// AddInMemoryCommandDispatcher
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IGenocsBuilder AddInMemoryCommandDispatcher(this IGenocsBuilder builder)
        {
            builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
            return builder;

        }
    }
}