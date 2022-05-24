using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Genocs.Core.Demo.ServiceBusAzure.Service;

public class MassTransitConsoleHostedService : IHostedService
{
    readonly IBusControl _bus;

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
