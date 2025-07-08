namespace FS.RabbitMQ.Core.Factories;

/// <summary>
/// Event arguments for connection creation failure events.
/// </summary>
public sealed class ConnectionFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionFailedEventArgs"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the connection that failed to create.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public ConnectionFailedEventArgs(string connectionName, Exception exception)
    {
        ConnectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        FailedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the name of the connection that failed to create.
    /// </summary>
    public string ConnectionName { get; }

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the time when the connection creation failed.
    /// </summary>
    public DateTimeOffset FailedAt { get; }
}
