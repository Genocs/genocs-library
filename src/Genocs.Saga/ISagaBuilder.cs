using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Saga;

public interface ISagaBuilder
{
    IServiceCollection Services { get; }

    ISagaBuilder UseInMemoryPersistence();

    ISagaBuilder UseSagaLog<TSagaLog>()
        where TSagaLog : ISagaLog;

    ISagaBuilder UseSagaStateRepository<TRepository>()
        where TRepository : ISagaStateRepository;
}
