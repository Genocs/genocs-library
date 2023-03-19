namespace Genocs.Core.Builders;

using Genocs.Core.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

/// <summary>
/// The Application builder
/// </summary>
public interface IGenocsBuilder
{
    /// <summary>
    /// Get the service collection
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Get the configuration
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// try to register a service by name
    /// </summary>
    /// <param name="name">Name of the service trying to register</param>
    /// <returns></returns>
    bool TryRegister(string name);

    /// <summary>
    /// Build the actions based on the service provider
    /// </summary>
    /// <param name="execute"></param>
    void AddBuildAction(Action<IServiceProvider> execute);
    void AddInitializer(IInitializer initializer);
    void AddInitializer<TInitializer>() where TInitializer : IInitializer;
    IServiceProvider Build();
}