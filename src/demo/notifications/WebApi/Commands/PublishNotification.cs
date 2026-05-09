using Genocs.Common.CQRS.Commands;

namespace Genocs.Notifications.WebApi.Commands;

public class PublishNotification(Guid notificationId, Guid customerId, string message) : ICommand
{
    public Guid NotificationId { get; } = notificationId == Guid.Empty ? Guid.NewGuid() : notificationId;
    public Guid CustomerId { get; } = customerId == Guid.Empty ? Guid.NewGuid() : customerId;
    public string Message { get; } = string.IsNullOrWhiteSpace(message) ? "Hello" : message;
}