using System.Collections.Concurrent;
using Genocs.Common.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.Builders;

/// <summary>
/// Genocs builder implementation.
/// </summary>
public sealed class GenocsBuilder : IGenocsBuilder
{
    private readonly ConcurrentDictionary<string, bool> _registry = new ConcurrentDictionary<string, bool>();
    private readonly List<Action<IServiceProvider>> _buildActions;
    private readonly IServiceCollection _services;
    IServiceCollection IGenocsBuilder.Services => _services;

    /// <summary>
    /// The configuration.
    /// </summary>
    public IConfiguration? Configuration { get; private set; }

    public WebApplicationBuilder? WebApplicationBuilder { get; private set; }

    private GenocsBuilder(IServiceCollection services, IConfiguration? configuration)
    {
        _services = services;
        Configuration = configuration;

        _buildActions = new List<Action<IServiceProvider>>();
        _services.AddSingleton<IStartupInitializer>(new StartupInitializer());
    }

    private GenocsBuilder(WebApplicationBuilder builder)
    {
        WebApplicationBuilder = builder;
        Configuration = builder.Configuration;

        _services = builder.Services;
        _buildActions = new List<Action<IServiceProvider>>();
        _services.AddSingleton<IStartupInitializer>(new StartupInitializer());
    }

    public static IGenocsBuilder Create(WebApplicationBuilder builder)
        => new GenocsBuilder(builder);

    public static IGenocsBuilder Create(IServiceCollection services, IConfiguration? configuration = null)
        => new GenocsBuilder(services, configuration);

    public bool TryRegister(string name)
        => _registry.TryAdd(name, true);

    public void AddBuildAction(Action<IServiceProvider> execute)
        => _buildActions.Add(execute);

    public void AddInitializer(IInitializer initializer)
        => AddBuildAction(sp =>
        {
            var startupInitializer = sp.GetRequiredService<IStartupInitializer>();
            startupInitializer.AddInitializer(initializer);
        });

    public void AddInitializer<TInitializer>()
        where TInitializer : IInitializer
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