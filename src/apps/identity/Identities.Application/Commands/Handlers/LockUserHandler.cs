using Genocs.Core.CQRS.Commands;
using Genocs.Identities.Application.Domain.Repositories;
using Genocs.Identities.Application.Events;
using Genocs.Identities.Application.Exceptions;
using Genocs.Identities.Application.Services;

namespace Genocs.Identities.Application.Commands.Handlers;

public sealed class LockUserHandler(IUserRepository userRepository, IMessageBroker messageBroker) : ICommandHandler<LockUser>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMessageBroker _messageBroker = messageBroker;

    public async Task HandleAsync(LockUser command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(command.UserId) ?? throw new UserNotFoundException(command.UserId);
        if (user.Lock())
        {
            await _userRepository.UpdateAsync(user);
            await _messageBroker.PublishAsync(new UserLocked(user.Id));
        }
    }
}