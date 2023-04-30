using Genocs.Core.CQRS.Commands;
using Genocs.Template.Application.Events;
using Genocs.Template.Application.Exceptions;
using Genocs.Template.Application.Domain.Repositories;
using Genocs.Template.Application.Services;

namespace Genocs.Template.Application.Commands.Handlers;

internal sealed class UnlockUserHandler : ICommandHandler<UnlockUser>
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageBroker _messageBroker;

    public UnlockUserHandler(IUserRepository userRepository, IMessageBroker messageBroker)
    {
        _userRepository = userRepository;
        _messageBroker = messageBroker;
    }

    public async Task HandleAsync(UnlockUser command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(command.UserId);
        if (user is null)
        {
            throw new UserNotFoundException(command.UserId);
        }

        if (user.Unlock())
        {
            await _userRepository.UpdateAsync(user);
            await _messageBroker.PublishAsync(new UserUnlocked(user.Id));
        }
    }
}