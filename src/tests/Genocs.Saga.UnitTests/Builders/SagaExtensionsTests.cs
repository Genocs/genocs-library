using Genocs.Saga.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
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
    }

    [Fact]
    public void AddSaga_WhenOnlyStateRepositoryIsOverridden_ShouldKeepDefaultSagaLog()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSaga(saga => saga.UseSagaStateRepository<MySagaStateRepository>());

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ISagaStateRepository>().ShouldBeOfType<MySagaStateRepository>();
        provider.GetRequiredService<ISagaLog>().ShouldBeOfType<InMemorySagaLog>();
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
    }

    private sealed class MySagaLog : ISagaLog
    {
        public Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
            => Task.FromResult<IEnumerable<ISagaLogData>>([]);

        public Task WriteAsync(ISagaLogData message)
            => Task.CompletedTask;
    }

    private sealed class MySagaStateRepository : ISagaStateRepository
    {
        public Task<ISagaState?> ReadAsync(SagaId id, Type type)
            => Task.FromResult<ISagaState?>(null);

        public Task WriteAsync(ISagaState state)
            => Task.CompletedTask;
    }
}