namespace FS.RabbitMQ.Core.Exceptions;

/// <summary>
/// Exception thrown when connection timeout occurs
/// </summary>
public class ConnectionTimeoutException : ConnectionException
{
    public TimeSpan Timeout { get; }

    public ConnectionTimeoutException(TimeSpan timeout) 
        : base($"Connection timeout after {timeout.TotalSeconds} seconds")
    {
        Timeout = timeout;
    }

    public ConnectionTimeoutException(TimeSpan timeout, Exception innerException) 
        : base($"Connection timeout after {timeout.TotalSeconds} seconds", innerException)
    {
        Timeout = timeout;
    }
}