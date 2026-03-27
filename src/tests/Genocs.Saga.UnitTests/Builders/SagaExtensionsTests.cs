using Genocs.Saga.Async;
using Genocs.Saga.Persistence;
using Microsoft.Extensions.DependencyInjection;
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

    private sealed class AssemblyRegisteredMessage;

    private sealed class AssemblyRegisteredSaga : Saga, ISagaStartAction<AssemblyRegisteredMessage>
    {
        public Task HandleAsync(AssemblyRegisteredMessage message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(AssemblyRegisteredMessage message, ISagaContext context)
            => Task.CompletedTask;
    }
}