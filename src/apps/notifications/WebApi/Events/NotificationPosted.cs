namespace Genocs.Notifications.WebApi.Events;

public class NotificationPosted
{
    public DefaultIdType NotificationId { get; }

    public NotificationPosted(DefaultIdType notificationId)
    {
        NotificationId = notificationId;
    }
}
