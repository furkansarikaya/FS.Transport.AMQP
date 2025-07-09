namespace FS.Transport.AMQP.Core.Exceptions;

/// <summary>
/// Exception thrown when queue-related errors occur
/// </summary>
public class QueueException : RabbitMQException
{
    public string? QueueName { get; }

    public QueueException(string message, string? queueName = null)
        : base(message, "QUEUE_ERROR")
    {
        QueueName = queueName;
        if (!string.IsNullOrEmpty(queueName))
        {
            WithContext("QueueName", queueName);
        }
    }

    public QueueException(string message, Exception innerException, string? queueName = null)
        : base(message, innerException, "QUEUE_ERROR")
    {
        QueueName = queueName;
        if (!string.IsNullOrEmpty(queueName))
        {
            WithContext("QueueName", queueName);
        }
    }
}