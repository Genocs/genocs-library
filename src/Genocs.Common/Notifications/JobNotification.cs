namespace Genocs.Common.Notifications;

/// <summary>
/// The JobNotification class represents a notification message that
/// contains information about a job, such as its progress and status.
/// It implements the INotificationMessage interface,
/// which allows it to be used with the INotificationSender interface to send notifications to clients.
/// </summary>
public class JobNotification : INotificationMessage
{
    private string _message = string.Empty;
    private decimal _progress;

    /// <summary>
    /// Gets or sets the message associated with the current instance.
    /// </summary>
    /// <remarks>Message must be non-empty and non-whitespace.</remarks>
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

    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    /// <remarks>The job identifier can be used to track the status and results of a job operation. This
    /// property may be null if the job has not been assigned an identifier.</remarks>
    public string JobId { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage of the operation.
    /// </summary>
    /// <remarks>
    /// Valid range is from 0 to 100, where 0 indicates no progress and 100 indicates completion.
    /// </remarks>
    public decimal Progress
    {
        get => _progress;
        set
        {
            if (value < 0m || value > 100m)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Progress must be between 0 and 100.");
            }

            _progress = value;
        }
    }
}