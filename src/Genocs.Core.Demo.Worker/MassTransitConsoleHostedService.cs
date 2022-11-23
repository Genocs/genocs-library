using MassTransit;

namespace Genocs.Core.Demo.Worker;

public class MassTransitConsoleHostedService : IHostedService
{
    private readonly IBusControl _bus;

    public MassTransitConsoleHostedService(IBusControl bus)
    {
        _bus = bus;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        //await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        //return _bus.StopAsync(cancellationToken);
    }
}
