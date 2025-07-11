namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when connection timeout occurs
/// </summary>
public class ConnectionTimeoutException : ConnectionException
{
    /// <summary>
    /// Gets the timeout duration that was exceeded
    /// </summary>
    /// <value>The timeout duration that caused the exception</value>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTimeoutException"/> class with a specified timeout
    /// </summary>
    /// <param name="timeout">The timeout duration that was exceeded</param>
    public ConnectionTimeoutException(TimeSpan timeout) 
        : base($"Connection timeout after {timeout.TotalSeconds} seconds")
    {
        Timeout = timeout;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTimeoutException"/> class with a specified timeout and inner exception
    /// </summary>
    /// <param name="timeout">The timeout duration that was exceeded</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConnectionTimeoutException(TimeSpan timeout, Exception innerException) 
        : base($"Connection timeout after {timeout.TotalSeconds} seconds", innerException)
    {
        Timeout = timeout;
    }
}