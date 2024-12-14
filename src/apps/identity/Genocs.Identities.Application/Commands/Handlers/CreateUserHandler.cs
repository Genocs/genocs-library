using Genocs.Core.CQRS.Commands;
using Genocs.Identities.Application.Domain.Constants;
using Genocs.Identities.Application.Domain.Entities;
using Genocs.Identities.Application.Domain.Exceptions;
using Genocs.Identities.Application.Domain.Repositories;
using Genocs.Identities.Application.Events;
using Genocs.Identities.Application.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Genocs.Identities.Application.Commands.Handlers;

internal sealed class CreateUserHandler(IUserRepository userRepository, IPasswordService passwordService,
    IMessageBroker messageBroker, ILogger<CreateUserHandler> logger) : ICommandHandler<CreateUser>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IMessageBroker _messageBroker = messageBroker;
    private readonly ILogger<CreateUserHandler> _logger = logger;

    private static readonly Regex EmailRegex = new Regex(
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task HandleAsync(CreateUser command, CancellationToken cancellationToken = default)
    {
        if (!EmailRegex.IsMatch(command.Email))
        {
            _logger.LogError($"Invalid email: {command.Email}");
            throw new InvalidEmailException(command.Email);
        }

        var user = await _userRepository.GetByEmailAsync(command.Email);
        if (user is { })
        {
            _logger.LogError($"Email already in use: {command.Email}");
            throw new EmailInUseException(command.Email);
        }

        user = await _userRepository.GetByNameAsync(command.Name);
        if (user is { })
        {
            _logger.LogError($"Name already in use: {command.Name}");
            throw new NameInUseException(command.Name);
        }

        string password = _passwordService.Hash(command.Password);
        user = new User(command.UserId, command.Email, command.Name, password, new List<string> { Roles.User }, DateTime.UtcNow, command.Permissions);
        await _userRepository.AddAsync(user);
        _logger.LogInformation($"Created an account for the user with ID: '{user.Id}'.");
        await _messageBroker.PublishAsync(new UserCreated(user.Id, user.Name, user.Roles));
    }
}