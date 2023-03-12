namespace Genocs.Core.Builders
{
    using Genocs.Core.Types;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// The Application builder
    /// </summary>
    public interface IGenocsBuilder
    {
        IServiceCollection Services { get; }
        IConfiguration Configuration { get; }
        bool TryRegister(string name);
        void AddBuildAction(Action<IServiceProvider> execute);
        void AddInitializer(IInitializer initializer);
        void AddInitializer<TInitializer>() where TInitializer : IInitializer;
        IServiceProvider Build();
    }
}