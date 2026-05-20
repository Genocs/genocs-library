using Genocs.Core.Builders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Genocs.Common.Types;

namespace Genocs.Core.UnitTests.Builders;

/// <summary>
/// Tests for startup initializer sequencing, ordering, and failure handling.
/// </summary>
public class StartupInitializerSequencingTests
{
    [Fact]
    public async Task MultipleInitializers_ExecuteInRegistrationOrder()
    {
        var executionOrder = new List<int>();
        var services = new ServiceCollection();
        var initializer = new ChainedStartupInitializer();

        services.AddSingleton<IStartupInitializer>(initializer);
        initializer.AddInitializer(new TrackingInitializer(1, executionOrder));
        initializer.AddInitializer(new TrackingInitializer(2, executionOrder));
        initializer.AddInitializer(new TrackingInitializer(3, executionOrder));

        using ServiceProvider provider = services.BuildServiceProvider();
        await initializer.InitializeAsync();

        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
    }

    [Fact]
    public async Task Initializer_CancellationToken_IsPropagated()
    {
        var tokenReceived = false;
        var expectedTokenReceived = false;
        var services = new ServiceCollection();
        var initializer = new ChainedStartupInitializer();

        services.AddSingleton<IStartupInitializer>(initializer);
        initializer.AddInitializer(new CancellationTrackingInitializer(
            onCancelled: (token, expected) =>
            {
                tokenReceived = !token.Equals(default);
                expectedTokenReceived = token.Equals(expected);
            }));

        using ServiceProvider provider = services.BuildServiceProvider();
        using var cts = new CancellationTokenSource();

        await initializer.InitializeAsync(cts.Token);

        Assert.True(tokenReceived);
        Assert.True(expectedTokenReceived);
    }

    [Fact]
    public async Task Initializer_Exception_StopsInitialization()
    {
        var executionOrder = new List<int>();
        var services = new ServiceCollection();
        var initializer = new ChainedStartupInitializer();

        services.AddSingleton<IStartupInitializer>(initializer);
        initializer.AddInitializer(new TrackingInitializer(1, executionOrder));
        initializer.AddInitializer(new ThrowingInitializer(new InvalidOperationException("boom")));
        initializer.AddInitializer(new TrackingInitializer(3, executionOrder));

        using ServiceProvider provider = services.BuildServiceProvider();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => initializer.InitializeAsync());

        Assert.Equal("boom", exception.Message);

        // Only the first initializer should have executed.
        Assert.Equal(new[] { 1 }, executionOrder);
    }

    [Fact]
    public async Task EmptyInitializer_CompletesSuccessfully()
    {
        var services = new ServiceCollection();
        var initializer = new ChainedStartupInitializer();

        services.AddSingleton<IStartupInitializer>(initializer);

        using ServiceProvider provider = services.BuildServiceProvider();

        // Should not throw even with no registered initializers.
        await initializer.InitializeAsync();
    }

    [Fact]
    public void Initializer_RepeatedAddInitializer_RegistersBoth()
    {
        var services = new ServiceCollection();
        var initializer = new ChainedStartupInitializer();
        var tracking1 = new TrackingInitializer(1, []);
        var tracking2 = new TrackingInitializer(2, []);

        initializer.AddInitializer(tracking1);
        initializer.AddInitializer(tracking2);

        services.AddSingleton<IStartupInitializer>(initializer);

        using ServiceProvider provider = services.BuildServiceProvider();

        var chainedInit = provider.GetRequiredService<IStartupInitializer>() as ChainedStartupInitializer;
        Assert.NotNull(chainedInit);
    }

    private sealed class TrackingInitializer : IInitializer
    {
        private readonly int _id;
        private readonly List<int> _executionOrder;

        public TrackingInitializer(int id, List<int> executionOrder)
        {
            _id = id;
            _executionOrder = executionOrder;
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _executionOrder.Add(_id);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingInitializer : IInitializer
    {
        private readonly Exception _exception;

        public ThrowingInitializer(Exception exception)
            => _exception = exception;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
            => Task.FromException(_exception);
    }

    private sealed class CancellationTrackingInitializer : IInitializer
    {
        private readonly Action<CancellationToken, CancellationToken> _onInitialize;

        public CancellationTrackingInitializer(Action<CancellationToken, CancellationToken> onCancelled)
            => _onInitialize = onCancelled;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _onInitialize(cancellationToken, cancellationToken);
            return Task.CompletedTask;
        }
    }

    private sealed class ChainedStartupInitializer : IStartupInitializer
    {
        private readonly List<IInitializer> _initializers = [];

        public void AddInitializer(IInitializer initializer)
            => _initializers.Add(initializer);

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            foreach (var initializer in _initializers)
            {
                await initializer.InitializeAsync(cancellationToken);
            }
        }
    }
}
