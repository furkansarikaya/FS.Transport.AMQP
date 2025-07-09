namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Exception thrown when RabbitMQ client-specific errors occur
/// </summary>
public class RabbitMQClientException : RabbitMQException
{
    public RabbitMQClientException(string message) 
        : base(message, "CLIENT_ERROR")
    {
    }

    public RabbitMQClientException(string message, Exception innerException) 
        : base(message, innerException, "CLIENT_ERROR")
    {
    }
}