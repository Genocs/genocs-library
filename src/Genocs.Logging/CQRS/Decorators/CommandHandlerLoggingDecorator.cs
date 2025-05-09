using Genocs.Common.Types;
using Genocs.Core.CQRS.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace Genocs.Logging.CQRS.Decorators;

[Decorator]
internal sealed class CommandHandlerLoggingDecorator<TCommand>(ICommandHandler<TCommand> handler, ILogger<CommandHandlerLoggingDecorator<TCommand>> logger, IServiceProvider serviceProvider) : ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _handler = handler;
    private readonly ILogger<CommandHandlerLoggingDecorator<TCommand>> _logger = logger;
    private readonly IMessageToLogTemplateMapper _mapper = serviceProvider.GetService<IMessageToLogTemplateMapper>() ?? new EmptyMessageToLogTemplateMapper();

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var template = _mapper.Map(command);

        if (template is null)
        {
            await _handler.HandleAsync(command, cancellationToken);
            return;
        }

        try
        {
            Log(command, template.Before);
            await _handler.HandleAsync(command, cancellationToken);
            Log(command, template.After);
        }
        catch (Exception ex)
        {
            string? exceptionTemplate = template.GetExceptionTemplate(ex);

            Log(command, exceptionTemplate, isError: true);
            throw;
        }
    }

    private void Log(TCommand command, string? message, bool isError = false)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (isError)
        {
            _logger.LogError(Smart.Format(message, command));
        }
        else
        {
            _logger.LogInformation(Smart.Format(message, command));
        }
    }

    private class EmptyMessageToLogTemplateMapper : IMessageToLogTemplateMapper
    {
        public HandlerLogTemplate? Map<TMessage>(TMessage message)
            where TMessage : class => null;
    }
}