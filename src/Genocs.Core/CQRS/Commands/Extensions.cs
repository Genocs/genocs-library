namespace Genocs.Core.CQRS.Commands
{
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
        /// <param name="services"></param>
        public static void AddCommandHandlers(this IServiceCollection services)
        {
            services.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>))
                        .WithoutAttribute(typeof(DecoratorAttribute)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());
        }

        /// <summary>
        /// AddInMemoryCommandDispatcher
        /// </summary>
        /// <param name="services"></param>
        public static void AddInMemoryCommandDispatcher(this IServiceCollection services)
        {
            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        }
    }
}