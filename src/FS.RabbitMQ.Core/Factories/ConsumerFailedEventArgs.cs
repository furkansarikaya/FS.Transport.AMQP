namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Event arguments for consumer creation failure events.
/// </summary>
public sealed class ConsumerFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumerFailedEventArgs"/> class.
    /// </summary>
    /// <param name="queueName">The name of the queue for which consumer creation failed.</param>
    /// <param name="messageType">The type of messages that was being consumed.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public ConsumerFailedEventArgs(string queueName, Type messageType, Exception exception)
    {
        QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        FailedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the name of the queue for which consumer creation failed.
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Gets the type of messages that was being consumed.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the time when the consumer creation failed.
    /// </summary>
    public DateTimeOffset FailedAt { get; }
}