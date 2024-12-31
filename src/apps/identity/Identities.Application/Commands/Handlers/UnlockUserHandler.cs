using Genocs.Core.CQRS.Commands;
using Genocs.Identities.Application.Domain.Repositories;
using Genocs.Identities.Application.Events;
using Genocs.Identities.Application.Exceptions;
using Genocs.Identities.Application.Services;

namespace Genocs.Identities.Application.Commands.Handlers;

internal sealed class UnlockUserHandler(IUserRepository userRepository, IMessageBroker messageBroker)
    : ICommandHandler<UnlockUser>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMessageBroker _messageBroker = messageBroker;

    public async Task HandleAsync(UnlockUser command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(command.UserId)
            ?? throw new UserNotFoundException(command.UserId);

        if (user.Unlock())
        {
            await _userRepository.UpdateAsync(user);
            await _messageBroker.PublishAsync(new UserUnlocked(user.Id));
        }
    }
}