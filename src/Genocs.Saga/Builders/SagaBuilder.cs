using Genocs.Saga.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Saga.Builders;

internal class SagaBuilder : ISagaBuilder
{
    public IServiceCollection Services { get; }

    public SagaBuilder(IServiceCollection services)
        => Services = services;

    public ISagaBuilder UseInMemoryPersistence()
    {
        Services.AddSingleton(typeof(ISagaStateRepository), typeof(InMemorySagaStateRepository));
        Services.AddSingleton(typeof(ISagaLog), typeof(InMemorySagaLog));
        return this;
    }

    public ISagaBuilder UseSagaLog<TSagaLog>()
        where TSagaLog : ISagaLog
    {
        Services.AddTransient(typeof(ISagaLog), typeof(TSagaLog));
        return this;
    }

    public ISagaBuilder UseSagaStateRepository<TRepository>()
        where TRepository : ISagaStateRepository
    {
        Services.AddTransient(typeof(ISagaStateRepository), typeof(TRepository));
        return this;
    }
}
