namespace Genocs.SignalR.WebApi.Event;

public class NotificationPosted
{

    public Guid NotificationId { get; }


    public NotificationPosted(Guid notificationId)
    {
        NotificationId = notificationId;
    }
}
