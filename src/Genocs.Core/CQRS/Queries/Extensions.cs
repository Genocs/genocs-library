namespace Genocs.Core.CQRS.Queries
{
    using Dispatchers;
    using Genocs.Core.Types;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// AddQueryHandlers
        /// </summary>
        /// <param name="services"></param>
        public static void AddQueryHandlers(this IServiceCollection services)
        {
            services.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))
                        .WithoutAttribute(typeof(DecoratorAttribute)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());
        }

        /// <summary>
        /// AddInMemoryQueryDispatcher
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static void AddInMemoryQueryDispatcher(this IServiceCollection services)
        {
            services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        }
    }
}