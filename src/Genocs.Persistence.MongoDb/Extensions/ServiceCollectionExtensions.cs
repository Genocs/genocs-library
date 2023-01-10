using Genocs.Persistence.MongoDb.Options;
using Genocs.Persistence.MongoDb.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Genocs.Persistence.MongoDb.Extensions
{
    /// <summary>
    /// ServiceCollectionExtension kfor mongoDb Repository setup
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Setup the MongoDatabase
        /// </summary>
        /// <param name="services">Service Collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns></returns>
        public static IServiceCollection AddMongoDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.Position));
            services.TryAddScoped<IMongoDatabaseProvider, MongoDatabaseProvider>();
            services.AddScoped(typeof(IMongoDbRepository<>), typeof(MongoDbRepository<>));
            return services;
        }

        /// <summary>
        /// Register all the MongoDb databases
        /// </summary>
        /// <param name="services">Service Collection</param>
        /// <param name="assembly">Assembly to scan</param>
        /// <param name="lifetime">Kind of ServiceLifetime</param>
        /// <returns></returns>
        public static IServiceCollection RegisterRepositories(this IServiceCollection services, Assembly assembly,
                          ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            services
                .Scan(s => s.FromAssemblyDependencies(assembly)
                .AddClasses(c => c.AssignableTo(typeof(IMongoDbRepository<>)))
                .AsImplementedInterfaces()
                .WithLifetime(lifetime));

            return services;
        }
    }
}
