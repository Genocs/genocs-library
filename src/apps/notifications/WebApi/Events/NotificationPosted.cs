namespace Genocs.Notifications.WebApi.Events;

public class NotificationPosted(DefaultIdType notificationId)
{
    public DefaultIdType NotificationId { get; } = notificationId;
}
