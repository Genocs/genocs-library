namespace Genocs.Core.Builders
{
    using Genocs.Core.Types;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Genocs builder implementation
    /// </summary>
    public sealed class GenocsBuilder : IGenocsBuilder
    {
        private readonly ConcurrentDictionary<string, bool> _registry = new ConcurrentDictionary<string, bool>();
        private readonly List<Action<IServiceProvider>> _buildActions;
        private readonly IServiceCollection _services;
        IServiceCollection IGenocsBuilder.Services => _services;

        public IConfiguration Configuration { get; }

        private GenocsBuilder(IServiceCollection services, IConfiguration configuration)
        {
            _buildActions = new List<Action<IServiceProvider>>();
            _services = services;
            _services.AddSingleton<IStartupInitializer>(new StartupInitializer());
            Configuration = configuration;
        }

        public static IGenocsBuilder Create(IServiceCollection services, IConfiguration configuration = null)
            => new GenocsBuilder(services, configuration);

        public bool TryRegister(string name) => _registry.TryAdd(name, true);

        public void AddBuildAction(Action<IServiceProvider> execute)
            => _buildActions.Add(execute);

        public void AddInitializer(IInitializer initializer)
            => AddBuildAction(sp =>
            {
                var startupInitializer = sp.GetRequiredService<IStartupInitializer>();
                startupInitializer.AddInitializer(initializer);
            });

        public void AddInitializer<TInitializer>() where TInitializer : IInitializer
            => AddBuildAction(sp =>
            {
                var initializer = sp.GetRequiredService<TInitializer>();
                var startupInitializer = sp.GetRequiredService<IStartupInitializer>();
                startupInitializer.AddInitializer(initializer);
            });

        public IServiceProvider Build()
        {
            var serviceProvider = _services.BuildServiceProvider();
            _buildActions.ForEach(a => a(serviceProvider));
            return serviceProvider;
        }
    }
}