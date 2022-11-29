using MassTransit;

namespace Genocs.Core.Demo.Worker;

/// <summary>
/// General purpose worker. Please check the link below for further informations
/// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-7.0&tabs=visual-studio
/// </summary>
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
