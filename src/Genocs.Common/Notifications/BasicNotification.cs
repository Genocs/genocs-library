namespace Genocs.Common.Notifications;

public class BasicNotification : INotificationMessage
{
    /// <summary>
    /// Specifies the type of label to display, indicating the nature or severity of a message.
    /// </summary>
    /// <remarks>Use this enumeration to categorize messages for display in user interfaces or logs. The
    /// values represent common message types, such as informational messages, successful operations, warnings, and
    /// errors. Selecting the appropriate label type can help users quickly understand the context or importance of a
    /// message.</remarks>
    public enum LabelType
    {
        Information,
        Success,
        Warning,
        Error
    }

    public string? Message { get; set; }

    public LabelType Label { get; set; }
}