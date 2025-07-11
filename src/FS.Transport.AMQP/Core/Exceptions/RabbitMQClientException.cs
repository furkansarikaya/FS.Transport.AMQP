namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Exception thrown when RabbitMQ client-related errors occur
/// </summary>
public class RabbitMQClientException : RabbitMQException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQClientException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public RabbitMQClientException(string message) 
        : base(message, "CLIENT_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQClientException"/> class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public RabbitMQClientException(string message, Exception innerException) 
        : base(message, innerException, "CLIENT_ERROR")
    {
    }
}