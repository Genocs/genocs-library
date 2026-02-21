using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Saga.Managers;

internal sealed class SagaSeeker : ISagaSeeker
{
    private readonly IServiceProvider _serviceProvider;

    public SagaSeeker(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public IEnumerable<ISagaAction<TMessage>> Seek<TMessage>()
        => _serviceProvider.GetService<IEnumerable<ISagaAction<TMessage>>>()
        .Union(_serviceProvider.GetService<IEnumerable<ISagaStartAction<TMessage>>>())
        .GroupBy(s => s.GetType())
        .Select(g => g.First())
        .Distinct();
}
