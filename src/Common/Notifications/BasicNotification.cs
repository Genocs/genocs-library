namespace Genocs.Common.Notifications;

public class BasicNotification : INotificationMessage
{
    private string _message = string.Empty;

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

    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    /// <remarks>
    /// Message must be non-empty and non-whitespace.
    /// </remarks>
    public required string Message
    {
        get => _message;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Notification message cannot be null or whitespace.", nameof(value));
            }

            _message = value;
        }
    }

    public LabelType Label { get; set; }
}