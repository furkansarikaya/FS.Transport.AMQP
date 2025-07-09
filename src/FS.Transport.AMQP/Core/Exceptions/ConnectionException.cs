namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Exception thrown when connection-related errors occur
/// </summary>
public class ConnectionException : RabbitMQException
{
    public ConnectionException(string message) 
        : base(message, "CONNECTION_ERROR")
    {
    }

    public ConnectionException(string message, Exception innerException) 
        : base(message, innerException, "CONNECTION_ERROR")
    {
    }
}