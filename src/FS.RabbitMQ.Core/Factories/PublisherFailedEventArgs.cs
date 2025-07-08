namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Event arguments for publisher creation failure events.
/// </summary>
public sealed class PublisherFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherFailedEventArgs"/> class.
    /// </summary>
    /// <param name="messageType">The type of messages that was being published.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public PublisherFailedEventArgs(Type messageType, Exception exception)
    {
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        FailedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the type of messages that was being published.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the time when the publisher creation failed.
    /// </summary>
    public DateTimeOffset FailedAt { get; }
}
