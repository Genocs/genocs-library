namespace Genocs.Core.CQRS.Commands.Dispatchers
{
    using Genocs.Core.CQRS.Commands;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// CommandDispatcher implementation
    /// </summary>
    internal sealed class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>();
            await handler.HandleAsync(command, cancellationToken);
        }
    }
}