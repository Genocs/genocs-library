using Genocs.Core.CQRS.Commands;
using Genocs.Core.Demo.Users.Application.Domain.Repositories;
using Genocs.Core.Demo.Users.Application.Events;
using Genocs.Core.Demo.Users.Application.Exceptions;
using Genocs.Core.Demo.Users.Application.Services;

namespace Genocs.Core.Demo.Users.Application.Commands.Handlers;

internal sealed class LockUserHandler : ICommandHandler<LockUser>
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageBroker _messageBroker;

    public LockUserHandler(IUserRepository userRepository, IMessageBroker messageBroker)
    {
        _userRepository = userRepository;
        _messageBroker = messageBroker;
    }

    public async Task HandleAsync(LockUser command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(command.UserId);
        if (user is null)
        {
            throw new UserNotFoundException(command.UserId);
        }

        if (user.Lock())
        {
            await _userRepository.UpdateAsync(user);
            await _messageBroker.PublishAsync(new UserLocked(user.Id));
        }
    }
}