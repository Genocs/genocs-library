using Genocs.Saga.Builders;
using Genocs.Saga.Async;
using Genocs.Saga.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Genocs.Saga.UnitTests.Builders;

public class SagaBuilderTests
{
    [Fact]
    public void UseInMemoryPersistence_Registers_InMemorySagaStateRepository_As_Singleton()
    {
        _builder.UseInMemoryPersistence();

        _services.ShouldContain(sd =>
            sd.ServiceType == typeof(ISagaStateRepository) &&
            sd.ImplementationType == typeof(InMemorySagaStateRepository) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseInMemoryPersistence_Registers_InMemorySagaLog_As_Singleton()
    {
        _builder.UseInMemoryPersistence();

        _services.ShouldContain(sd =>
            sd.ServiceType == typeof(ISagaLog) &&
            sd.ImplementationType == typeof(InMemorySagaLog) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseSagaLog_Registers_GivenImplementation_As_Transient()
    {
        _builder.UseSagaLog<MySagaLog>();

        _services.ShouldContain(sd =>
            sd.ServiceType == typeof(ISagaLog) &&
            sd.ImplementationType == typeof(MySagaLog) &&
            sd.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void UseSagaStateRepository_Registers_GivenImplementation_As_Transient()
    {
        _builder.UseSagaStateRepository<MySagaStateRepository>();

        _services.ShouldContain(sd =>
            sd.ServiceType == typeof(ISagaStateRepository) &&
            sd.ImplementationType == typeof(MySagaStateRepository) &&
            sd.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void UseInProcessExecutionLock_Registers_InProcessSagaExecutionLock_As_Singleton()
    {
        _builder.UseInProcessExecutionLock();

        _services.ShouldContain(sd =>
            sd.ServiceType == typeof(ISagaExecutionLock) &&
            sd.ImplementationType == typeof(InProcessSagaExecutionLock) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void DisableInProcessExecutionLock_Registers_NoOpSagaExecutionLock_As_Singleton()
    {
        _builder.DisableInProcessExecutionLock();

        _services.ShouldContain(sd =>
            sd.ServiceType == typeof(ISagaExecutionLock) &&
            sd.ImplementationType == typeof(NoOpSagaExecutionLock) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    #region ARRANGE

    private readonly IServiceCollection _services;
    private readonly ISagaBuilder _builder;

    public SagaBuilderTests()
    {
        _services = new ServiceCollection();
        _builder = new SagaBuilder(_services);
    }

    public class MySagaLog : ISagaLog
    {
        public Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(ISagaLogData message)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOutcomeAsync(SagaId id, Type type, string entryId, SagaLogEntryOutcome outcome)
        {
            throw new NotImplementedException();
        }
    }

    public class MySagaStateRepository : ISagaStateRepository
    {
        public Task<ISagaState?> ReadAsync(SagaId id, Type type)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(ISagaState state)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
