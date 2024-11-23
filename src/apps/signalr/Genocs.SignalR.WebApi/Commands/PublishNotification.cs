using Genocs.Core.CQRS.Commands;

namespace Genocs.SignalR.WebApi.Commands;

public class PublishNotification : ICommand
{
    public Guid NotificationId { get; }
    public Guid CustomerId { get; }
    public string Message { get; }

    public PublishNotification(Guid notificationId, Guid customerId, string message)
    {
        NotificationId = notificationId == Guid.Empty ? Guid.NewGuid() : notificationId;
        CustomerId = customerId == Guid.Empty ? Guid.NewGuid() : customerId;
        Message = string.IsNullOrWhiteSpace(message) ? "Hello" : message;
    }
}