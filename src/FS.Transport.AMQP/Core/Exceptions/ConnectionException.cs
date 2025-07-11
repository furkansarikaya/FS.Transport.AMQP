namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Exception thrown when connection-related errors occur
/// </summary>
public class ConnectionException : RabbitMQException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ConnectionException(string message) 
        : base(message, "CONNECTION_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionException"/> class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConnectionException(string message, Exception innerException) 
        : base(message, innerException, "CONNECTION_ERROR")
    {
    }
}