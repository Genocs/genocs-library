using Genocs.Common.Types;
using Genocs.Core.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Core.UnitTests.Builders;

public class StartupInitializerWiringIntegrationTests
{
    [Fact]
    public async Task BuilderBuild_WiresInitializers_InRegistrationOrder()
    {
        var services = new ServiceCollection();
        var trackerInstance = new InitializerExecutionTracker();
        services.AddSingleton(trackerInstance);
        services.AddSingleton<FirstInitializer>();
        services.AddSingleton<ThirdInitializer>();

        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        builder.AddInitializer<FirstInitializer>();
        builder.AddInitializer(new SecondInitializer(trackerInstance, 2));
        builder.AddInitializer<ThirdInitializer>();

        using ServiceProvider provider = services.BuildServiceProvider();
        builder.Build(provider);

        var startupInitializer = provider.GetRequiredService<IStartupInitializer>();
        var tracker = provider.GetRequiredService<InitializerExecutionTracker>();

        await startupInitializer.InitializeAsync();

        Assert.Equal([1, 2, 3], tracker.ExecutionOrder);
    }

    [Fact]
    public async Task BuilderBuild_InitializersRun_ThroughUseGenocsAsyncPath()
    {
        var services = new ServiceCollection();
        services.AddSingleton<InitializerExecutionTracker>();
        services.AddSingleton<FirstInitializer>();

        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());
        builder.AddInitializer<FirstInitializer>();

        using ServiceProvider provider = services.BuildServiceProvider();
        builder.Build(provider);

        var app = new Microsoft.AspNetCore.Builder.ApplicationBuilder(provider);
        var tracker = provider.GetRequiredService<InitializerExecutionTracker>();

        await app.UseGenocsAsync();

        Assert.Equal([1], tracker.ExecutionOrder);
    }

    public sealed class InitializerExecutionTracker
    {
        public List<int> ExecutionOrder { get; } = [];
    }

    public sealed class FirstInitializer : IInitializer
    {
        private readonly InitializerExecutionTracker _tracker;

        public FirstInitializer(InitializerExecutionTracker tracker)
            => _tracker = tracker;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _tracker.ExecutionOrder.Add(1);
            return Task.CompletedTask;
        }
    }

    public sealed class SecondInitializer : IInitializer
    {
        private readonly InitializerExecutionTracker _tracker;
        private readonly int _order;

        public SecondInitializer(InitializerExecutionTracker tracker, int order)
        {
            _tracker = tracker;
            _order = order;
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _tracker.ExecutionOrder.Add(_order);
            return Task.CompletedTask;
        }
    }

    public sealed class ThirdInitializer : IInitializer
    {
        private readonly InitializerExecutionTracker _tracker;

        public ThirdInitializer(InitializerExecutionTracker tracker)
            => _tracker = tracker;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _tracker.ExecutionOrder.Add(3);
            return Task.CompletedTask;
        }
    }

}
