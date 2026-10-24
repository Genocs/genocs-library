using Genocs.Saga.Async;
using Genocs.Saga.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using System.Reflection;
using Xunit;

namespace Genocs.Saga.UnitTests.Builders;

public class SagaExtensionsTests
{
    [Fact]
    public void AddSaga_WhenBuildCallbackIsEmpty_ShouldKeepInMemoryPersistence()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(_ => { });

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ISagaStateRepository>().ShouldBeOfType<InMemorySagaStateRepository>();
        provider.GetRequiredService<ISagaLog>().ShouldBeOfType<InMemorySagaLog>();
        provider.GetRequiredService<ISagaExecutionLock>().ShouldBeOfType<InProcessSagaExecutionLock>();
    }

    [Fact]
    public void AddSaga_WhenOnlyStateRepositoryIsOverridden_ShouldKeepDefaultSagaLog()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(saga => saga.UseSagaStateRepository<MySagaStateRepository>());

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ISagaStateRepository>().ShouldBeOfType<MySagaStateRepository>();
        provider.GetRequiredService<ISagaLog>().ShouldBeOfType<InMemorySagaLog>();
        provider.GetRequiredService<ISagaExecutionLock>().ShouldBeOfType<InProcessSagaExecutionLock>();
    }

    [Fact]
    public void AddSaga_WhenBothPersistenceImplementationsAreOverridden_ShouldResolveCustomImplementations()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(saga =>
        {
            saga.UseSagaStateRepository<MySagaStateRepository>();
            saga.UseSagaLog<MySagaLog>();
        });

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ISagaStateRepository>().ShouldBeOfType<MySagaStateRepository>();
        provider.GetRequiredService<ISagaLog>().ShouldBeOfType<MySagaLog>();
        provider.GetRequiredService<ISagaExecutionLock>().ShouldBeOfType<InProcessSagaExecutionLock>();
    }

    [Fact]
    public void AddSaga_WhenInProcessLockingIsDisabled_ShouldResolveNoOpExecutionLock()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(saga => saga.DisableInProcessExecutionLock());

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ISagaExecutionLock>().ShouldBeOfType<NoOpSagaExecutionLock>();
    }

    [Fact]
    public void AddSaga_WhenExplicitAssemblyContainsSaga_ShouldRegisterIt()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(typeof(AssemblyRegisteredSaga).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetServices<ISagaStartAction<AssemblyRegisteredMessage>>()
            .ShouldContain(saga => saga.GetType() == typeof(AssemblyRegisteredSaga));
    }

    [Fact]
    public void AddSaga_WhenExplicitAssemblyDoesNotContainSaga_ShouldNotRegisterIt()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(typeof(object).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetServices<ISagaStartAction<AssemblyRegisteredMessage>>().ShouldBeEmpty();
    }

    [Fact]
    public void AddSaga_WhenBuildCallbackAndAssembliesAreProvided_ShouldApplyBoth()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(saga => saga.DisableInProcessExecutionLock(), typeof(AssemblyRegisteredSaga).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ISagaExecutionLock>().ShouldBeOfType<NoOpSagaExecutionLock>();
        provider.GetServices<ISagaStartAction<AssemblyRegisteredMessage>>()
            .ShouldContain(saga => saga.GetType() == typeof(AssemblyRegisteredSaga));
    }

    [Fact]
    public void AddSaga_ShouldRegisterDiagnosticsReport()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(typeof(AssemblyRegisteredSaga).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();

        SagaRegistrationDiagnostics diagnostics = provider.GetRequiredService<SagaRegistrationDiagnostics>();

        diagnostics.StateRepositoryType.ShouldBe(typeof(InMemorySagaStateRepository));
        diagnostics.SagaLogType.ShouldBe(typeof(InMemorySagaLog));
        diagnostics.ExecutionLockType.ShouldBe(typeof(InProcessSagaExecutionLock));
        diagnostics.ScannedAssemblies.ShouldContain(typeof(AssemblyRegisteredSaga).Assembly);
        diagnostics.Sagas.ShouldContain(saga =>
            saga.SagaType == typeof(AssemblyRegisteredSaga) &&
            saga.Bindings.Any(binding => binding.MessageType == typeof(AssemblyRegisteredMessage) && binding.StartsSaga));
    }

    [Fact]
    public async Task AddSaga_WhenHostedStartupDiagnosticsRuns_ShouldReportDiscoveryWarnings()
    {
        IServiceCollection services = new ServiceCollection();
        var loggerProvider = new TestLoggerProvider();

        services.AddSingleton<ILoggerFactory>(new LoggerFactory([loggerProvider]));
        services.AddSaga(typeof(object).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();

        var hostedServices = provider.GetServices<IHostedService>();
        SagaStartupDiagnosticsHostedService diagnosticsService = hostedServices
            .ShouldHaveSingleItem()
            .ShouldBeOfType<SagaStartupDiagnosticsHostedService>();

        await diagnosticsService.StartAsync(CancellationToken.None);

        loggerProvider.Messages.ShouldContain(message => message.LogLevel == LogLevel.Warning && message.Message.Contains("no saga types were discovered", StringComparison.Ordinal));
    }

    private sealed class MySagaLog : ISagaLog
    {
        public Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
            => Task.FromResult<IEnumerable<ISagaLogData>>([]);

        public Task WriteAsync(ISagaLogData message)
            => Task.CompletedTask;

        public Task UpdateOutcomeAsync(SagaId id, Type type, string entryId, SagaLogEntryOutcome outcome)
            => Task.CompletedTask;
    }

    private sealed class MySagaStateRepository : ISagaStateRepository
    {
        public Task<ISagaState?> ReadAsync(SagaId id, Type type)
            => Task.FromResult<ISagaState?>(null);

        public Task WriteAsync(ISagaState state)
            => Task.CompletedTask;
    }

    public sealed class AssemblyRegisteredMessage;

    public sealed class AssemblyRegisteredSaga : Saga, ISagaStartAction<AssemblyRegisteredMessage>
    {
        public Task HandleAsync(AssemblyRegisteredMessage message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(AssemblyRegisteredMessage message, ISagaContext context)
            => Task.CompletedTask;
    }

    private sealed class TestLoggerProvider : ILoggerProvider
    {
        public List<TestLogMessage> Messages { get; } = [];

        public ILogger CreateLogger(string categoryName)
            => new TestLogger(Messages);

        public void Dispose()
        {
        }
    }

    private sealed class TestLogger(List<TestLogMessage> messages) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            messages.Add(new TestLogMessage(logLevel, formatter(state, exception)));
        }
    }

    private sealed record TestLogMessage(LogLevel LogLevel, string Message);
}