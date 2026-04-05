using Genocs.Common.Types;
using Genocs.Core.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Core.UnitTests.Builders;

public class UseGenocsExtensionsTests
{
    [Fact]
    public async Task UseGenocsAsync_PassesCancellationTokenToInitializer()
    {
        var services = new ServiceCollection();
        var initializer = new TestStartupInitializer();
        services.AddSingleton<IStartupInitializer>(initializer);

        using ServiceProvider provider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(provider);
        using var cts = new CancellationTokenSource();

        IApplicationBuilder result = await app.UseGenocsAsync(cts.Token);

        Assert.Same(app, result);
        Assert.True(initializer.InitializeCalled);
        Assert.Equal(cts.Token, initializer.ReceivedToken);
    }

    [Fact]
    public void UseGenocs_RethrowsInitializerException()
    {
        var services = new ServiceCollection();
        var expected = new InvalidOperationException("boom");
        var initializer = new ThrowingStartupInitializer(expected);
        services.AddSingleton<IStartupInitializer>(initializer);

        using ServiceProvider provider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(provider);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => app.UseGenocs());
        Assert.Same(expected, ex);
    }

    [Fact]
    public async Task StartupInitializer_EmitsDiagnostics_WhenEnabled()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build()).AddCoreDiagnostics();
        builder.AddInitializer(new TestInitializer());

        using ServiceProvider provider = services.BuildServiceProvider();
        builder.Build(provider);

        IStartupInitializer startupInitializer = provider.GetRequiredService<IStartupInitializer>();
        await startupInitializer.InitializeAsync();

        CoreDiagnosticsState diagnostics = provider.GetRequiredService<CoreDiagnosticsState>();

        Assert.Contains(diagnostics.Messages, m => m.Contains("Registered startup initializer instance", StringComparison.Ordinal));
        Assert.Contains(diagnostics.Messages, m => m.Contains("Startup initializer added", StringComparison.Ordinal));
        Assert.Contains(diagnostics.Messages, m => m.Contains("Executing 1 startup initializer(s).", StringComparison.Ordinal));
    }

    private sealed class TestStartupInitializer : IStartupInitializer
    {
        public bool InitializeCalled { get; private set; }

        public CancellationToken ReceivedToken { get; private set; }

        public void AddInitializer(IInitializer initializer)
        {
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            InitializeCalled = true;
            ReceivedToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingStartupInitializer : IStartupInitializer
    {
        private readonly Exception _exception;

        public ThrowingStartupInitializer(Exception exception)
            => _exception = exception;

        public void AddInitializer(IInitializer initializer)
        {
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
            => Task.FromException(_exception);
    }

    private sealed class TestInitializer : IInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
