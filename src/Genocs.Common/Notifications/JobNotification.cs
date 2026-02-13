namespace Genocs.Common.Notifications;

/// <summary>
/// The JobNotification class represents a notification message that
/// contains information about a job, such as its progress and status.
/// It implements the INotificationMessage interface,
/// which allows it to be used with the INotificationSender interface to send notifications to clients.
/// </summary>
public class JobNotification : INotificationMessage
{
    /// <summary>
    /// Gets or sets the message associated with the current instance.
    /// </summary>
    /// <remarks>This property can be used to provide additional context or information related to the
    /// instance. It may be null if no message is set.</remarks>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    /// <remarks>The job identifier can be used to track the status and results of a job operation. This
    /// property may be null if the job has not been assigned an identifier.</remarks>
    public string? JobId { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage of the operation, represented as a decimal value between 0 and 100.
    /// </summary>
    /// <remarks>The value should be between 0 and 100, where 0 indicates no progress and 100 indicates
    /// completion. Setting a value outside this range may result in unexpected behavior.</remarks>
    public decimal Progress { get; set; }
}