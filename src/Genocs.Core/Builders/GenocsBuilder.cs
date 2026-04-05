using System.Collections.Concurrent;
using Genocs.Common.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

    /// <summary>
    /// The web application builder.
    /// </summary>
    public WebApplicationBuilder? WebApplicationBuilder { get; private set; }

    /// <summary>
    /// The Genocs builder constructor.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The IConfiguration.</param>
    private GenocsBuilder(IServiceCollection services, IConfiguration? configuration)
    {
        _services = services;
        Configuration = ResolveConfiguration(services, configuration);

        _buildActions = [];
        _services.TryAddSingleton(Configuration);
        _services.AddSingleton<IStartupInitializer, StartupInitializer>();
    }

    private GenocsBuilder(WebApplicationBuilder builder)
    {
        WebApplicationBuilder = builder;
        Configuration = builder.Configuration;

        _services = builder.Services;
        _buildActions = [];
        _services.AddSingleton<IStartupInitializer, StartupInitializer>();
    }

    public static IGenocsBuilder Create(WebApplicationBuilder builder)
        => new GenocsBuilder(builder);

    public static IGenocsBuilder Create(IServiceCollection services, IConfiguration? configuration = null)
        => new GenocsBuilder(services, configuration);

    private static IConfiguration ResolveConfiguration(IServiceCollection services, IConfiguration? explicitConfiguration)
    {
        if (explicitConfiguration is not null)
        {
            return explicitConfiguration;
        }

        var descriptor = services.LastOrDefault(service => service.ServiceType == typeof(IConfiguration));
        if (descriptor?.ImplementationInstance is IConfiguration existingConfiguration)
        {
            return existingConfiguration;
        }

        return new ConfigurationBuilder().Build();
    }

    public bool TryRegister(string name)
        => _registry.TryAdd(name, true);

    public void AddBuildAction(Action<IServiceProvider> execute)
        => _buildActions.Add(execute);

    public void AddInitializer(IInitializer initializer)
        => AddBuildAction(sp =>
        {
            var startupInitializer = sp.GetRequiredService<IStartupInitializer>();
            startupInitializer.AddInitializer(initializer);
            CoreDiagnosticsRuntime.Info(sp, $"Registered startup initializer instance '{initializer.GetType().FullName}'.");
        });

    public void AddInitializer<TInitializer>()
        where TInitializer : IInitializer
        => AddBuildAction(sp =>
        {
            var initializer = sp.GetRequiredService<TInitializer>();
            var startupInitializer = sp.GetRequiredService<IStartupInitializer>();
            startupInitializer.AddInitializer(initializer);
            CoreDiagnosticsRuntime.Info(sp, $"Registered startup initializer type '{typeof(TInitializer).FullName}'.");
        });

    /// <summary>
    /// Executes deferred build actions against the final application service provider.
    /// </summary>
    /// <param name="serviceProvider">The final application service provider.</param>
    public void Build(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _buildActions.ForEach(action => action(serviceProvider));
    }
}