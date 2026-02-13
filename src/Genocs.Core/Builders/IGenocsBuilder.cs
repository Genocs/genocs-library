using Genocs.Common.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.Builders;

/// <summary>
/// The Application builder. The Genocs builder is used to build the application.
/// It provides a way to register services and build the service provider.
/// </summary>
public interface IGenocsBuilder
{
    /// <summary>
    /// Get the service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Get the configuration.
    /// </summary>
    IConfiguration? Configuration { get; }

    /// <summary>
    /// Gets the builder used to configure the web application during startup.
    /// </summary>
    /// <remarks>This property provides access to the underlying WebApplicationBuilder instance, which is used
    /// to register services, configure middleware, and set up other application settings before the application is
    /// built and run. Accessing this property allows advanced customization of the application's startup
    /// process.</remarks>
    WebApplicationBuilder? WebApplicationBuilder { get; }

    /// <summary>
    /// try to register a service by name.
    /// </summary>
    /// <param name="name">Name of the service trying to register.</param>
    /// <returns>True if the service was successfully registered, otherwise false.</returns>
    bool TryRegister(string name);

    /// <summary>
    /// Build the actions based on the service provider.
    /// </summary>
    /// <param name="execute">The action to execute with the service provider.</param>
    void AddBuildAction(Action<IServiceProvider> execute);

    void AddInitializer(IInitializer initializer);

    void AddInitializer<TInitializer>()
        where TInitializer : IInitializer;

    /// <summary>
    /// Creates and returns a service provider that can be used to resolve registered services.
    /// </summary>
    /// <remarks>This method is typically used in dependency injection scenarios to obtain service instances
    /// as needed.</remarks>
    /// <returns>An <see cref="IServiceProvider"/> instance that provides access to the configured services.</returns>
    IServiceProvider Build();
}