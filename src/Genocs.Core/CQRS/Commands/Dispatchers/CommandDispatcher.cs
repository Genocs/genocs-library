using Genocs.Common.Cqrs.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.Cqrs.Commands.Dispatchers;

/// <summary>
/// CommandDispatcher implementation.
/// </summary>
internal sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>();
        await handler.HandleAsync(command, cancellationToken);
    }
}