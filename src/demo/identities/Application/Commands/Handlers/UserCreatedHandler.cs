using Genocs.Common.CQRS.Events;
using Genocs.Identities.Application.Events;
using Microsoft.Extensions.Logging;

namespace Genocs.Identities.Application.Commands.Handlers;

public sealed class UserCreatedHandler(ILogger<UserCreatedHandler> logger) : IEventHandler<UserCreated>
{
    private readonly ILogger<UserCreatedHandler> _logger = logger;
    public async Task HandleAsync(UserCreated message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {EventName} for UserId: {UserId}", nameof(UserCreated), message.UserId);
    }
}