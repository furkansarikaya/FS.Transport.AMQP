namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Event arguments for channel creation failure events.
/// </summary>
public sealed class ChannelFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelFailedEventArgs"/> class.
    /// </summary>
    /// <param name="connectionId">The ID of the connection.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public ChannelFailedEventArgs(string connectionId, Exception exception)
    {
        ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        FailedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the ID of the connection.
    /// </summary>
    public string ConnectionId { get; }

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the time when the channel creation failed.
    /// </summary>
    public DateTimeOffset FailedAt { get; }
}
